using System.Collections.Generic;
using System.Threading.Tasks;
using ec.edu.monster.models;

namespace ec.edu.monster.services
{
    public interface IEurekaApi
    {
        Task<bool> ValidarIngreso(string usuario, string password);
        Task<List<Movimiento>> TraerMovimientos(string cuenta);
        Task<OperacionCuentaResponse> RegDeposito(string cuenta, decimal importe);
        Task<OperacionCuentaResponse> RegRetiro(string cuenta, decimal importe);
        Task<OperacionCuentaResponse> RegTransferencia(string origen, string destino, decimal importe);
        Task<string> ProbarConexion();
    }
}
