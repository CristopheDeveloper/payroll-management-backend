using System.Globalization;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class ParametroServicio : IParametroServicio
{
    private static readonly string[] Porcentajes =
    [
        ClavesParametro.IgssLaboral, ClavesParametro.IgssPatronal
    ];

    private static readonly string[] Cuentas =
    [
        ClavesParametro.CuentaSueldosPorPagar, ClavesParametro.CuentaIgssPatronalPorPagar
    ];

    private readonly IParametroRepositorio _parametroRepositorio;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public ParametroServicio(IParametroRepositorio parametroRepositorio, IUnidadTrabajo unidadTrabajo)
    {
        _parametroRepositorio = parametroRepositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<List<ParametroDto>> ObtenerTodos()
    {
        var parametros = await _parametroRepositorio.ObtenerTodos();
        return parametros.Select(ADto).ToList();
    }

    public async Task<ParametroDto> ObtenerPorId(int idParametro)
    {
        var parametro = await _parametroRepositorio.ObtenerPorId(idParametro)
            ?? throw new ReglaNegocioException("El parámetro no existe.", 404);

        return ADto(parametro);
    }

    public async Task<decimal> ObtenerSalarioMinimo()
    {
        return await ObtenerNumero(ClavesParametro.SalarioMinimo);
    }

    public async Task<decimal> ObtenerBonificacionIncentivo()
    {
        return await ObtenerNumero(ClavesParametro.BonificacionIncentivo);
    }

    private async Task<decimal> ObtenerNumero(string clave)
    {
        var parametro = await _parametroRepositorio.ObtenerPorClave(clave);
        if (parametro == null || !decimal.TryParse(parametro.Valor, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var numero))
        {
            throw new InvalidOperationException($"El parámetro {clave} no existe o no es un número.");
        }
        return numero;
    }

    public async Task<ParametroDto> Editar(int idParametro, ParametroEditarDto dto)
    {
        var parametro = await _parametroRepositorio.ObtenerParaModificar(idParametro)
            ?? throw new ReglaNegocioException("El parámetro no existe.", 404);

        var valor = dto.Valor.Trim();
        Validar(parametro, valor);

        parametro.Valor = valor;
        parametro.Descripcion = dto.Descripcion.Trim();
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(parametro);
    }

    private static void Validar(Parametro parametro, string valor)
    {
        switch (parametro.TipoDato)
        {
            case "ENTERO":
                if (!int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var entero))
                {
                    throw new ReglaNegocioException("El valor debe ser un número entero mayor o igual a 0.");
                }
                ValidarRango(parametro.Clave, entero);
                break;

            case "DECIMAL":
                if (!decimal.TryParse(valor, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var numero))
                {
                    throw new ReglaNegocioException("El valor debe ser un número mayor o igual a 0, con punto como separador decimal.");
                }
                ValidarRango(parametro.Clave, numero);
                break;

            case "TEXTO":
                if (string.IsNullOrWhiteSpace(valor))
                {
                    throw new ReglaNegocioException("El valor no puede estar vacío.");
                }
                if (Cuentas.Contains(parametro.Clave) && !EsCuenta(valor))
                {
                    throw new ReglaNegocioException("La cuenta contable debe tener 4 dígitos.");
                }
                break;
        }
    }

    private static bool EsCuenta(string valor)
    {
        return valor.Length == 4 && valor.All(char.IsAsciiDigit);
    }

    private static void ValidarRango(string clave, decimal numero)
    {
        if (clave == ClavesParametro.DiasMes && (numero < 1 || numero > 31))
        {
            throw new ReglaNegocioException("Los días del mes deben estar entre 1 y 31.");
        }
        if (clave == ClavesParametro.HorasMes && (numero < 1 || numero > 999))
        {
            throw new ReglaNegocioException("Las horas del mes deben estar entre 1 y 999.");
        }
        if (clave == ClavesParametro.FactorHoraExtra && (numero < 1 || numero > 10))
        {
            throw new ReglaNegocioException("El factor de hora extra debe estar entre 1 y 10.");
        }
        if (Porcentajes.Contains(clave) && numero > 100)
        {
            throw new ReglaNegocioException("El porcentaje debe estar entre 0 y 100.");
        }
        if ((Porcentajes.Contains(clave) || clave == ClavesParametro.FactorHoraExtra) && numero != Math.Round(numero, 4))
        {
            throw new ReglaNegocioException("El valor debe tener como máximo 4 decimales.");
        }
        if (clave == ClavesParametro.BonificacionIncentivo && numero != Math.Round(numero, 2))
        {
            throw new ReglaNegocioException("La bonificación debe tener como máximo 2 decimales.");
        }
    }

    private static ParametroDto ADto(Parametro parametro)
    {
        return new ParametroDto
        {
            IdParametro = parametro.IdParametro,
            Clave = parametro.Clave,
            Valor = parametro.Valor,
            TipoDato = parametro.TipoDato,
            Descripcion = parametro.Descripcion
        };
    }
}
