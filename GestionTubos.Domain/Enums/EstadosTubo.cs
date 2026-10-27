namespace GestionTubos.Domain.Enums;
public enum EstadoContenido { Lleno=1, Vacio=2, ParcialmenteLleno=3, Desconocido=4 }
public enum EstadoDisponibilidad { Disponible=1, Reservado=2, EnReparto=3, EnPoderCliente=4, EnCarga=5, PendienteInspeccion=6, EnMantenimiento=7, EnPruebaHidraulica=8, DadoDeBaja=9, Extraviado=10, PreparadoParaCarga=11, EnviadoACarga=12, EnBateria=13 }
public enum EstadoFisico { Bueno=1, ConObservaciones=2, Danado=3, Inutilizable=4 }
public enum TipoPropietarioTubo { Empresa=1, Cliente=2, Proveedor=3, OtroTercero=4 }
