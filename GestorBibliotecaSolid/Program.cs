public class Libro
{
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string ISBN { get; set; }
    public bool Disponible { get; set; }

    public Libro(string titulo, string autor, string isbn)
    {
        Titulo = titulo;
        Autor = autor;
        ISBN = isbn;
        Disponible = true;
    }
}

public class Usuario
{
    public string Nombre { get; set; }
    public string ID { get; set; }

    public Usuario(string nombre, string id)
    {
        Nombre = nombre;
        ID = id;
    }
}

public class Prestamo
{
    public Libro Libro { get; set; }
    public Usuario Usuario { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucion { get; set; }
    public bool Devuelto { get; set; }

    public Prestamo(Libro libro, Usuario usuario)
    {
        Libro = libro;
        Usuario = usuario;
        FechaPrestamo = DateTime.Now;
        FechaDevolucion = FechaPrestamo.AddDays(14); // Devolución en 14 días
        Devuelto = false;
    }
}

public abstract class CalcularMulta
{
    public abstract decimal Calcular(int diasAtraso);
}

public class MultaEstandar : CalcularMulta
{
    private const decimal tarifaDiaria = 1.0m; // Tarifa diaria fija

    public override decimal Calcular(int diasAtraso)
    {
        return diasAtraso * tarifaDiaria;
    }
}

public class MultaEstudiantil : CalcularMulta
{
    private const decimal tarifaDiaria = 0.5m; // Tarifa diaria reducida para estudiantes

    public override decimal Calcular(int diasAtraso)
    {
        return diasAtraso * tarifaDiaria;
    }
}

public class MultaPremium : CalcularMulta
{
    private const decimal tarifaDiaria = 0.2m; // Tarifa diaria reducida para miembros premium

    public override decimal Calcular(int diasAtraso)
    {
        return diasAtraso * tarifaDiaria;
    }
}

public abstract class Notificador
{
    public abstract void Notificar(string mensaje, string destinatario);
}

public class NotificadorEmail : Notificador
{
    public override void Notificar(string mensaje, string destinatario)
    {
        Console.WriteLine($"Enviando email a {destinatario}: {mensaje}");
    }
}

public class NotificadorSMS : Notificador
{
    public override void Notificar(string mensaje, string destinatario)
    {
        Console.WriteLine($"Enviando SMS a {destinatario}: {mensaje}");
    }
}

public interface IReservable
{
    void Reservar(Usuario usuario);
}

public interface IPrestable
{
    void Prestar(Usuario usuario);
}
public interface IRenovable
{
    void Renovar(Prestamo prestamo);
}

public class GestorPrestamos
{
    private List<Prestamo> prestamos = new List<Prestamo>();
    private Notificador notificador;
    private CalcularMulta calculadorMulta;


    public GestorPrestamos(Notificador notificador, CalcularMulta calculadorMulta)
    {
        this.notificador = notificador;
        this.calculadorMulta = calculadorMulta;
        prestamos = new List<Prestamo>();
    }

    public Prestamo PrestarLibro(Libro libro, Usuario usuario)
    {
        if (libro.Disponible)
        {
            var prestamo = new Prestamo(libro, usuario);
            prestamos.Add(prestamo);
            libro.Disponible = false;
            notificador.Notificar($"El libro '{libro.Titulo}' ha sido prestado a {usuario.Nombre}.", usuario.ID);
            return prestamo;
        }
        else
        {
            notificador.Notificar($"El libro '{libro.Titulo}' no está disponible para préstamo.", usuario.ID);
            return null;
        }
       
    }

    public void DevolverLibro(Prestamo prestamo)
    {
        if (!prestamo.Devuelto)
        {
            prestamo.Devuelto = true;
            prestamo.Libro.Disponible = true;

            int diasAtraso = (DateTime.Now - prestamo.FechaDevolucion).Days;
            if (diasAtraso > 0)
            {
                decimal multa = calculadorMulta.Calcular(diasAtraso);
                notificador.Notificar($"El libro '{prestamo.Libro.Titulo}' ha sido devuelto con {diasAtraso} días de retraso. Multa: ${multa}.", prestamo.Usuario.ID);
            }
            else
            {
                notificador.Notificar($"El libro '{prestamo.Libro.Titulo}' ha sido devuelto a tiempo. Gracias, {prestamo.Usuario.Nombre}!", prestamo.Usuario.ID);
            }
        }
        else
        {
            notificador.Notificar($"El libro '{prestamo.Libro.Titulo}' ya ha sido devuelto.", prestamo.Usuario.ID);
        }
    }

    public class  Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SISTEMA DE BIBLIOTECA - PRINCIPIOS SOLID");
            Console.WriteLine(new string('=', 50));

            // Crear algunos libros y usuarios
            var libro1 = new Libro("1984", "George Orwell", "1234567890");
            var libro2 = new Libro("Cien Años de Soledad", "Gabriel García Márquez", "0987654321");
            var usuario1 = new Usuario("Alice", "U001");
            var usuario2 = new Usuario("Bob", "U002");

            // Crear gestores de préstamos con diferentes estrategias de notificación y cálculo de multas
            Console.WriteLine("GESTOR PARA USUARIOS REGULARES:");
            var gestor_regular = new GestorPrestamos(new NotificadorEmail(), new MultaEstandar());
             Console.WriteLine("GESTOR PARA ESTUDIANTES:");
            var gestor_estudiante = new GestorPrestamos(new NotificadorSMS(), new MultaEstudiantil());
             Console.WriteLine("GESTOR PARA VIP:");
            var gestor_premium = new GestorPrestamos(new NotificadorEmail(), new MultaPremium());

            // Prestar libros
             Console.WriteLine("REALIZANDO PRÉSTAMOS:");
            Prestamo prestam1 = gestor_regular.PrestarLibro(libro1, usuario1);
            Prestamo prestam2 = gestor_estudiante.PrestarLibro(libro2, usuario2);
            //Prestamo prestam3 = gestor_premium.PrestarLibro(libro1, usuario2);

             Console.WriteLine("SIMULANDO DEVOLUCIÓN TARDÍA:");
            // Simular devolución con retraso
            System.Threading.Thread.Sleep(2000); // Simula el paso del tiempo
            prestam1.FechaDevolucion = DateTime.Now.AddDays(-5); // Simula que el libro se devolvió 5 días tarde
            //prestam2.FechaDevolucion = DateTime.Now.AddDays(-3); //
            gestor_regular.DevolverLibro(prestam1); // Devolver el primer libro
            //gestor_estudiante.DevolverLibro(prestam2); // Devolver el segundo libro 
            
            Console.WriteLine("DEVOLUCIÓN A TIEMPO:");
            //devolucion a tiempo
            gestor_premium.DevolverLibro(prestam2); // Devolver el tercer libro a   

        }
    }
}





