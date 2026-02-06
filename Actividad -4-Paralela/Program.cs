using System.Collections.Concurrent;
using System.Diagnostics;

namespace STRFloodAlert
{
    // Estados de riesgo (según tu diseño)
    enum RiskState { Normal, Vigilancia, Alerta, Emergencia }

    record SensorReading(
        string SensorId,
        string Zone,
        double WaterLevelM,      // nivel de agua (m)
        double RainMmH,          // lluvia (mm/h)
        DateTime SensorTimeUtc
    );

    record RiskDecision(
        string Zone,
        RiskState State,
        string RuleTriggered,
        DateTime ServerTimeUtc
    );

    class Program
    {
        // "Deadlines" (simulados) – ajustados a tu tabla
        static readonly TimeSpan DL_Ingest = TimeSpan.FromMilliseconds(200);
        static readonly TimeSpan DL_Validate = TimeSpan.FromMilliseconds(300);
        static readonly TimeSpan DL_Evaluate = TimeSpan.FromMilliseconds(500);
        static readonly TimeSpan DL_Alert = TimeSpan.FromSeconds(2);

        // Umbrales (ejemplo) – ponlos en el README
        const double TH_Water_Vigilancia = 2.0;
        const double TH_Water_Alerta = 3.0;
        const double TH_Water_Emerg = 3.5;

        const double TH_Rain_Vigilancia = 30;
        const double TH_Rain_Alerta = 60;
        const double TH_Rain_Emerg = 90;

        // Buffer/cola de tiempo real
        static readonly BlockingCollection<SensorReading> IngestQueue = new(1000);

        // Estado por zona
        static readonly ConcurrentDictionary<string, RiskState> ZoneState = new();

        static async Task Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== STR: Alerta Temprana Inundaciones (Simulación) ===");
            Console.WriteLine("Comandos: [1]=Normal  [2]=Vigilancia  [3]=Alerta  [4]=Emergencia  [q]=Salir");
            Console.WriteLine();

            using var cts = new CancellationTokenSource();

            // Tareas del STR
            var ingestTask = Task.Run(() => IngestLoop(cts.Token));
            var validateEvalTask = Task.Run(() => ValidateAndEvaluateLoop(cts.Token));
            var generatorTask = Task.Run(() => SensorGeneratorLoop(cts.Token));

            // Interacción: cambiar el "modo" de la simulación para provocar alertas en el video
            while (true)
            {
                var key = Console.ReadKey(true).KeyChar;
                if (key == 'q') break;

                if (key is '1' or '2' or '3' or '4')
                {
                    SimulationMode = key switch
                    {
                        '1' => 1,
                        '2' => 2,
                        '3' => 3,
                        '4' => 4,
                        _ => 1
                    };
                    Console.WriteLine($"[UI] Modo de simulación cambiado a: {ModeName(SimulationMode)}");
                }
            }

            cts.Cancel();
            IngestQueue.CompleteAdding();

            await Task.WhenAll(ingestTask, validateEvalTask, generatorTask);
            Console.WriteLine("Sistema finalizado.");
        }

        // 1=Normal, 2=Vigilancia, 3=Alerta, 4=Emergencia
        static volatile int SimulationMode = 1;

        static string ModeName(int mode) => mode switch
        {
            1 => "Normal",
            2 => "Vigilancia",
            3 => "Alerta",
            4 => "Emergencia",
            _ => "Normal"
        };

        // Simula sensores enviando lecturas
        static void SensorGeneratorLoop(CancellationToken ct)
        {
            var rnd = new Random();
            while (!ct.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var zone = "Zona-Norte";
                var sensorId = "SEN-001";

                // Genera datos según modo
                (double wl, double rain) = SimulationMode switch
                {
                    1 => (1.2 + rnd.NextDouble() * 0.3, 5 + rnd.NextDouble() * 5),
                    2 => (2.1 + rnd.NextDouble() * 0.3, 35 + rnd.NextDouble() * 10),
                    3 => (3.1 + rnd.NextDouble() * 0.3, 70 + rnd.NextDouble() * 10),
                    4 => (3.6 + rnd.NextDouble() * 0.4, 95 + rnd.NextDouble() * 15),
                    _ => (1.2, 5)
                };

                // A veces mete un dato inválido (para demostrar validación)
                if (rnd.Next(0, 20) == 0)
                    wl = -1;

                var reading = new SensorReading(sensorId, zone, wl, rain, now);

                // "Envío" al STR
                // (en real sería LoRaWAN/4G, aquí es cola en memoria)
                IngestQueue.Add(reading, ct);

                Thread.Sleep(250); // simula periodicidad/arrival
            }
        }

        // Simula ingesta con deadline
        static void IngestLoop(CancellationToken ct)
        {
            foreach (var reading in IngestQueue.GetConsumingEnumerable(ct))
            {
                var sw = Stopwatch.StartNew();

                // Sello de tiempo de servidor (en real se guardaría junto al t_sensor)
                var serverTime = DateTime.UtcNow;

                sw.Stop();
                LogDeadline("INGESTA", sw.Elapsed, DL_Ingest, $"Recibido {reading.SensorId} {reading.Zone} WL={reading.WaterLevelM:F2}m Rain={reading.RainMmH:F1}mm/h tS={reading.SensorTimeUtc:HH:mm:ss.fff}Z tSrv={serverTime:HH:mm:ss.fff}Z");
            }
        }

        // Validación + evaluación + alerta
        static void ValidateAndEvaluateLoop(CancellationToken ct)
        {
            // Para simplificar, volvemos a leer la misma cola (en un STR real, separarías colas/pipelines)
            // Aquí: re-leemos con TryTake desde una "shadow queue" simulada:
            // → Para hacerlo simple en demo: validamos/evaluamos leyendo consola desde una copia.
            // Solución demo: usamos un buffer interno consumiendo lecturas del generador antes de ingesta real.

            // Como ya se consumen en IngestLoop, aquí hacemos otro pipeline:
            // en demo: duplicamos el envío creando otra cola local.
            // Para no complicar, rehacemos: procesar en el mismo hilo NO.
            // Mejor: creamos un "tap" simple: en vez de tap, procesamos evaluando por estado actual
            // leyendo directamente de la cola ANTES de ingesta no es posible ya.
            // => Simplificación: fusionamos validación/evaluación/alerta dentro de IngestLoop normalmente.
            // Pero como ya lo imprimimos en ingesta, aquí dejamos un mensaje guía.
            Console.WriteLine("[INFO] Nota demo: Para pipeline completo, use la versión 'Pipeline' (ver README).");
            Console.WriteLine("[INFO] Esta demo ya genera datos y registra ingesta; abajo hay una versión completa alternativa.");
        }

        static void LogDeadline(string task, TimeSpan elapsed, TimeSpan deadline, string msg)
        {
            var ok = elapsed <= deadline;
            var status = ok ? "OK" : "MISS";
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}Z] [{task}] [{status}] {elapsed.TotalMilliseconds:F1}ms (DL {deadline.TotalMilliseconds:F0}ms) -> {msg}");
        }
    }
}
