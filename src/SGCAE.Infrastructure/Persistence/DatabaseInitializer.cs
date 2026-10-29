using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();
            await SeedDataAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error al migrar o inicializar la base de datos SQL Server.");
            throw;
        }
    }

    private static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.AccessPoints.AnyAsync())
            return;

        logger.LogInformation("Sembrando datos iniciales del caso de estudio SGCAE...");

        // 1. Puntos de acceso físicos (RF-06)
        var recepcion = AccessPoint.Create(
            "AP-REC-01",
            "Recepción Principal",
            "Edificio Administrativo - Piso 1",
            "Torniquete principal de entrada y salida de personal.",
            isRestrictedArea: false,
            isEnabled: true);

        var parqueadero = AccessPoint.Create(
            "AP-PAR-01",
            "Parqueadero Corporativo",
            "Sótano 1",
            "Barrera vehicular y acceso peatonal de parqueadero.",
            isRestrictedArea: false,
            isEnabled: true);

        var bodega = AccessPoint.Create(
            "AP-BOD-01",
            "Bodega Central de Almacenamiento",
            "Zona Logística Norte",
            "Acceso principal a zona de inventario y despacho.",
            isRestrictedArea: true,
            isEnabled: true);

        var areaRestringida = AccessPoint.Create(
            "AP-RES-01",
            "Área Restringida - Cuarto de Servidores",
            "Edificio Administrativo - Piso 3",
            "Acceso biométrico y RFID de alta seguridad.",
            isRestrictedArea: true,
            isEnabled: true);

        var puertaMantenimiento = AccessPoint.Create(
            "AP-BOD-02",
            "Muelle de Carga Secundario",
            "Zona Logística Sur",
            "Puerta inhabilitada temporalmente por mantenimiento preventivo.",
            isRestrictedArea: true,
            isEnabled: false);

        await context.AccessPoints.AddRangeAsync(
            recepcion, parqueadero, bodega, areaRestringida, puertaMantenimiento);

        // 2. Empleados de prueba (RF-01)
        var empActivo = Employee.Create(
            "1020304050",
            "Carlos Andrés",
            "Martínez Gómez",
            "carlos.martinez@logistica-sgcae.com",
            "Coordinador de Operaciones Logísticas",
            "Logística y Almacenamiento");

        var empSeguridad = Employee.Create(
            "1098765432",
            "Laura Valentina",
            "Ríos Herrera",
            "laura.rios@logistica-sgcae.com",
            "Analista de Seguridad Física",
            "Seguridad Corporativa");

        var empInactivo = Employee.Create(
            "1055667788",
            "Jorge Eduardo",
            "Pineda Soto",
            "jorge.pineda@logistica-sgcae.com",
            "Auxiliar de Bodega",
            "Logística y Almacenamiento");

        //Emitimos credenciales antes de desactivar al tercer empleado para simular el caso real de vínculo laboral inactivo
        var credActivaCarlos = Credential.Issue("CRD-2026-0001", empActivo);
        var credActivaLaura = Credential.Issue("CRD-2026-0002", empSeguridad);
        var credEmpleadoInactivo = Credential.Issue("CRD-2026-0003", empInactivo);

        // Ahora desactivamos a Jorge Eduardo (vínculo laboral inactivo)
        empInactivo.Deactivate();

        await context.Employees.AddRangeAsync(empActivo, empSeguridad, empInactivo);
        await context.Credentials.AddRangeAsync(credActivaCarlos, credActivaLaura, credEmpleadoInactivo);

        // 3. Movimientos iniciales de ejemplo (RF-03 / RF-04)
        var logAutorizadoEntrada = AccessLog.EvaluateAccessAttempt(
            credActivaCarlos.Code.Value,
            credActivaCarlos,
            empActivo,
            recepcion,
            MovementType.Entry);

        var logAutorizadoBodega = AccessLog.EvaluateAccessAttempt(
            credActivaCarlos.Code.Value,
            credActivaCarlos,
            empActivo,
            bodega,
            MovementType.Entry);

        var logRechazadoEmpleadoInactivo = AccessLog.EvaluateAccessAttempt(
            credEmpleadoInactivo.Code.Value,
            credEmpleadoInactivo,
            empInactivo,
            recepcion,
            MovementType.Entry);

        var logRechazadoPuntoInhabilitado = AccessLog.EvaluateAccessAttempt(
            credActivaLaura.Code.Value,
            credActivaLaura,
            empSeguridad,
            puertaMantenimiento,
            MovementType.Entry);

        await context.AccessLogs.AddRangeAsync(
            logAutorizadoEntrada,
            logAutorizadoBodega,
            logRechazadoEmpleadoInactivo,
            logRechazadoPuntoInhabilitado);

        await context.SaveChangesAsync();
        logger.LogInformation("Datos semilla de SGCAE creados exitosamente.");
    }
}

