using System.Diagnostics;

namespace VectoresDinamicos;

/// <summary>Resultado de insertar n elementos con una estrategia de crecimiento.</summary>
public record ResultadoEstrategia(int N, string Estrategia, double TiempoMs, int Redimensiones, long Copias, long MemoriaKB);

/// <summary>
/// Lógica del Programa 1, separada del formulario para que pueda probarse y reutilizarse.
/// Un arreglo de .NET tiene tamaño fijo: "redimensionar" significa crear uno nuevo y copiar.
/// </summary>
public static class OperacionesVector
{
    /// <summary>Crea un vector del tamaño indicado con valores aleatorios entre 1 y 99.</summary>
    public static int[] Generar(int tamano, Random generador)
    {
        int[] resultado = new int[tamano];   // la memoria se reserva aquí, en tiempo de ejecución
        for (int i = 0; i < resultado.Length; i++)
        {
            resultado[i] = generador.Next(1, 100);
        }
        return resultado;
    }

    /// <summary>Hace "a mano" lo mismo que Array.Resize: crea un arreglo nuevo y copia lo que quepa.</summary>
    public static int[] RedimensionarManual(int[] origen, int nuevoTamano)
    {
        int[] destino = new int[nuevoTamano];
        int elementosACopiar = Math.Min(origen.Length, nuevoTamano);
        for (int i = 0; i < elementosACopiar; i++)
        {
            destino[i] = origen[i];
        }
        return destino;   // el arreglo de origen queda sin referencias y el GC lo liberará
    }

    /// <summary>Agrega elementos a una List y registra cada vez que cambia su capacidad interna.</summary>
    public static List<(int Elementos, int Capacidad)> CrecimientoDeList(int totalElementos)
    {
        var cambios = new List<(int, int)>();
        var lista = new List<int>();
        int capacidadAnterior = lista.Capacity;
        cambios.Add((0, lista.Capacity));

        for (int i = 1; i <= totalElementos; i++)
        {
            lista.Add(i);
            if (lista.Capacity != capacidadAnterior)
            {
                cambios.Add((lista.Count, lista.Capacity));
                capacidadAnterior = lista.Capacity;
            }
        }
        return cambios;
    }

    /// <summary>Ejecuta una estrategia y mide tiempo, redimensiones, copias y memoria reservada.</summary>
    public static ResultadoEstrategia Medir(int n, bool duplicar)
    {
        // Calentamiento para que la compilación JIT no afecte la medición.
        if (duplicar) CrecerDuplicando(100); else CrecerDeUnoEnUno(100);

        long bytesAntes = GC.GetAllocatedBytesForCurrentThread();
        var cronometro = Stopwatch.StartNew();
        var (redimensiones, copias) = duplicar ? CrecerDuplicando(n) : CrecerDeUnoEnUno(n);
        cronometro.Stop();
        long kilobytes = (GC.GetAllocatedBytesForCurrentThread() - bytesAntes) / 1024;

        string nombre = duplicar ? "B (x2)" : "A (+1)";
        return new ResultadoEstrategia(n, nombre, cronometro.Elapsed.TotalMilliseconds, redimensiones, copias, kilobytes);
    }

    /// <summary>Estrategia A: cada inserción crea un arreglo una posición más grande.</summary>
    private static (int, long) CrecerDeUnoEnUno(int n)
    {
        int[] datos = Array.Empty<int>();
        int redimensiones = 0;
        long copias = 0;

        for (int i = 0; i < n; i++)
        {
            copias += datos.Length;               // Array.Resize copia todo lo que ya había
            Array.Resize(ref datos, datos.Length + 1);
            redimensiones++;
            datos[i] = i;
        }
        return (redimensiones, copias);
    }

    /// <summary>Estrategia B: cuando el arreglo se llena, se duplica su capacidad.</summary>
    private static (int, long) CrecerDuplicando(int n)
    {
        int[] datos = new int[1];
        int cantidad = 0;
        int redimensiones = 0;
        long copias = 0;

        for (int i = 0; i < n; i++)
        {
            if (cantidad == datos.Length)
            {
                copias += datos.Length;
                Array.Resize(ref datos, datos.Length * 2);
                redimensiones++;
            }
            datos[cantidad++] = i;
        }
        return (redimensiones, copias);
    }
}