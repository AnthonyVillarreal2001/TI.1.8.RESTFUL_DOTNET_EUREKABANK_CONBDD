using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using WS_EB_DOTNET_REST_Servidor.Models;

namespace WS_EB_DOTNET_REST_Servidor.Services
{
    public class EurekaService
    {
        private readonly IConfiguration _config;
        public EurekaService(IConfiguration config) => _config = config;

        private SqlConnection GetConnection()
            => new SqlConnection(_config.GetConnectionString("EUREKABANK")!);

        // ===== LOGIN (igual que SOAP) =====
        private const string USUARIO = "MONSTER";
        private static readonly string PASSWORD_HASH = CrearHash("MONSTER9");

        public bool ValidarIngreso(string usuario, string password)
            => USUARIO.Equals(usuario) && PASSWORD_HASH.Equals(CrearHash(password));

        private static string CrearHash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // ===== LISTAR MOVIMIENTOS =====
        public IEnumerable<Movimiento> ListarMovimientos(string cuenta)
        {
            var lista = new List<Movimiento>();
            using var cn = GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT chr_cuencodigo, int_movinumero, dtt_movifecha,
                       chr_emplcodigo, chr_tipocodigo, dec_moviimporte, chr_cuenreferencia
                FROM dbo.Movimiento
                WHERE chr_cuencodigo = @cuenta
                ORDER BY dtt_movifecha DESC, int_movinumero DESC;", cn);
            cmd.Parameters.AddWithValue("@cuenta", cuenta);
            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Movimiento
                {
                    CuentaCodigo = dr.GetString(0),
                    Numero = dr.GetInt32(1),
                    Fecha = dr.GetDateTime(2),
                    EmpleadoCodigo = dr.GetString(3),
                    TipoCodigo = dr.GetString(4),
                    Importe = dr.GetDecimal(5),
                    ReferenciaCuenta = dr.IsDBNull(6) ? null : dr.GetString(6)
                });
            }
            return lista;
        }

        // ===== HELPERS TX =====
        private static int SiguienteMov(SqlConnection cn, SqlTransaction tx, string cuenta)
        {
            using var cmd = new SqlCommand(
                "SELECT ISNULL(MAX(int_movinumero),0)+1 FROM dbo.Movimiento WHERE chr_cuencodigo=@c;",
                cn, tx);
            cmd.Parameters.Add(new SqlParameter("@c", SqlDbType.Char, 8) { Value = cuenta });
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void VerificarCuentaActiva(SqlConnection cn, SqlTransaction tx, string cuenta)
        {
            using var cmd = new SqlCommand(
                "SELECT 1 FROM dbo.Cuenta WHERE chr_cuencodigo=@c AND vch_cuenestado='ACTIVO';",
                cn, tx);
            cmd.Parameters.Add(new SqlParameter("@c", SqlDbType.Char, 8) { Value = cuenta });
            var ok = cmd.ExecuteScalar();
            if (ok == null) throw new Exception($"La cuenta {cuenta} no existe o no está activa.");
        }

        private static decimal SaldoActual(SqlConnection cn, SqlTransaction tx, string cuenta)
        {
            using var cmd = new SqlCommand(
                "SELECT dec_cuensaldo FROM dbo.Cuenta WITH (UPDLOCK, ROWLOCK, HOLDLOCK) WHERE chr_cuencodigo=@c;",
                cn, tx);
            cmd.Parameters.Add(new SqlParameter("@c", SqlDbType.Char, 8) { Value = cuenta });
            var r = cmd.ExecuteScalar();
            if (r == null) throw new Exception($"Cuenta {cuenta} no encontrada.");
            return Convert.ToDecimal(r);
        }

        // ===== DEPÓSITO: retorna saldo actualizado (como SOAP) =====
        public decimal RegistrarDeposito(string cuenta, decimal importe, string empleado)
        {
            using var cn = GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();
            try
            {
                VerificarCuentaActiva(cn, tx, cuenta);
                var nro = SiguienteMov(cn, tx, cuenta);

                decimal nuevoSaldo;
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Cuenta
                    SET dec_cuensaldo = dec_cuensaldo + @imp, int_cuencontmov = int_cuencontmov + 1
                    OUTPUT inserted.dec_cuensaldo
                    WHERE chr_cuencodigo=@c AND vch_cuenestado='ACTIVO';", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@imp", importe);
                    cmd.Parameters.AddWithValue("@c", cuenta);
                    var r = cmd.ExecuteScalar();
                    if (r == null) throw new Exception("No se pudo actualizar la cuenta.");
                    nuevoSaldo = Convert.ToDecimal(r);
                }

                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.Movimiento
                      (chr_cuencodigo,int_movinumero,dtt_movifecha,chr_emplcodigo,chr_tipocodigo,dec_moviimporte,chr_cuenreferencia)
                    VALUES
                      (@c,@n,GETDATE(),@e,'003',@imp,NULL);", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@c", cuenta);
                    cmd.Parameters.AddWithValue("@n", nro);
                    cmd.Parameters.AddWithValue("@e", string.IsNullOrWhiteSpace(empleado) ? "0001" : empleado);
                    cmd.Parameters.AddWithValue("@imp", importe);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                return Math.Round(nuevoSaldo, 2);
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ===== RETIRO: retorna saldo actualizado (como SOAP) =====
        public decimal RegistrarRetiro(string cuenta, decimal importe, string empleado)
        {
            using var cn = GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();
            try
            {
                VerificarCuentaActiva(cn, tx, cuenta);
                var saldo = SaldoActual(cn, tx, cuenta);
                if (saldo < importe) throw new Exception("Saldo insuficiente.");
                var nro = SiguienteMov(cn, tx, cuenta);

                decimal nuevoSaldo;
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Cuenta
                    SET dec_cuensaldo = dec_cuensaldo - @imp, int_cuencontmov = int_cuencontmov + 1
                    OUTPUT inserted.dec_cuensaldo
                    WHERE chr_cuencodigo=@c AND vch_cuenestado='ACTIVO';", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@imp", importe);
                    cmd.Parameters.AddWithValue("@c", cuenta);
                    var r = cmd.ExecuteScalar();
                    if (r == null) throw new Exception("No se pudo actualizar la cuenta.");
                    nuevoSaldo = Convert.ToDecimal(r);
                }

                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.Movimiento
                      (chr_cuencodigo,int_movinumero,dtt_movifecha,chr_emplcodigo,chr_tipocodigo,dec_moviimporte,chr_cuenreferencia)
                    VALUES
                      (@c,@n,GETDATE(),@e,'004',@imp,NULL);", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@c", cuenta);
                    cmd.Parameters.AddWithValue("@n", nro);
                    cmd.Parameters.AddWithValue("@e", string.IsNullOrWhiteSpace(empleado) ? "0004" : empleado);
                    cmd.Parameters.AddWithValue("@imp", importe);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                return Math.Round(nuevoSaldo, 2);
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ===== TRANSFERENCIA: retorna saldo actualizado de ORIGEN (como SOAP) =====
        public decimal RegistrarTransferencia(string origen, string destino, decimal importe, string empleado)
        {
            using var cn = GetConnection();
            cn.Open();
            using var tx = cn.BeginTransaction();
            try
            {
                var a = string.CompareOrdinal(origen, destino) <= 0 ? origen : destino;
                var b = a == origen ? destino : origen;

                VerificarCuentaActiva(cn, tx, a);
                VerificarCuentaActiva(cn, tx, b);

                var saldoOri = SaldoActual(cn, tx, origen);
                if (saldoOri < importe) throw new Exception("Saldo insuficiente en cuenta origen.");

                var nroOri = SiguienteMov(cn, tx, origen);
                var nroDes = SiguienteMov(cn, tx, destino);

                decimal saldoOrigenNuevo;
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Cuenta
                    SET dec_cuensaldo = dec_cuensaldo - @imp, int_cuencontmov = int_cuencontmov + 1
                    OUTPUT inserted.dec_cuensaldo
                    WHERE chr_cuencodigo=@c AND vch_cuenestado='ACTIVO';", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@imp", importe);
                    cmd.Parameters.AddWithValue("@c", origen);
                    var r = cmd.ExecuteScalar();
                    if (r == null) throw new Exception("No se pudo debitar origen.");
                    saldoOrigenNuevo = Convert.ToDecimal(r);
                }

                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.Movimiento
                      (chr_cuencodigo,int_movinumero,dtt_movifecha,chr_emplcodigo,chr_tipocodigo,dec_moviimporte,chr_cuenreferencia)
                    VALUES
                      (@c,@n,GETDATE(),@e,'009',@imp,NULL);", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@c", origen);
                    cmd.Parameters.AddWithValue("@n", nroOri);
                    cmd.Parameters.AddWithValue("@e", string.IsNullOrWhiteSpace(empleado) ? "0004" : empleado);
                    cmd.Parameters.AddWithValue("@imp", importe);
                    cmd.Parameters.AddWithValue("@ref", destino);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Cuenta
                    SET dec_cuensaldo = dec_cuensaldo + @imp, int_cuencontmov = int_cuencontmov + 1
                    WHERE chr_cuencodigo=@c AND vch_cuenestado='ACTIVO';", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@imp", importe);
                    cmd.Parameters.AddWithValue("@c", destino);
                    if (cmd.ExecuteNonQuery() == 0) throw new Exception("No se pudo acreditar destino.");
                }

                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.Movimiento
                      (chr_cuencodigo,int_movinumero,dtt_movifecha,chr_emplcodigo,chr_tipocodigo,dec_moviimporte,chr_cuenreferencia)
                    VALUES
                      (@c,@n,GETDATE(),@e,'008',@imp,NULL);", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@c", destino);
                    cmd.Parameters.AddWithValue("@n", nroDes);
                    cmd.Parameters.AddWithValue("@e", string.IsNullOrWhiteSpace(empleado) ? "0004" : empleado);
                    cmd.Parameters.AddWithValue("@imp", importe);
                    cmd.Parameters.AddWithValue("@ref", origen);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                return Math.Round(saldoOrigenNuevo, 2); // saldo de ORIGEN
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
