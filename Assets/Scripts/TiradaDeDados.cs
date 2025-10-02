using System;

public class TiradaDeDados
{
    private static Random random = new Random();

    public static int TirarDados(int cantidadDados, int carasPorDado)
    {
        if (cantidadDados <= 0 || carasPorDado <= 0)
        {
            throw new ArgumentException("La cantidad de dados y caras por dado deben ser mayores que cero.");
        }

        int resultado = 0;

        for (int i = 0; i < cantidadDados; i++)
        {
            resultado += random.Next(1, carasPorDado + 1);
        }

       
        return resultado;
    }

    public static void Main()
    {
        // Ejemplo de uso
        int resultadoTirada = TirarDados(3, 6);
        Console.WriteLine($"Resultado de la tirada: {resultadoTirada}");
    }
}