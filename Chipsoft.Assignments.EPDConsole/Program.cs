namespace Chipsoft.Assignments.EPDConsole;

public class Program
{
    //Don't create EF migrations, use the reset db option
    //This deletes and recreates the db, this makes sure all tables exist

    private static void AddPatient()
    {
        //Do action
        //return to show menu again.
    }

    private static void ShowAppointment()
    {
    }

    private static void AddAppointment()
    {
    }

    private static void DeletePhysician()
    {
    }

    private static void AddPhysician()
    {
    }

    private static void DeletePatient()
    {
    }


    #region FreeCodeForAssignment

    static void Main(string[] args)
    {
        while (ShowMenu())
        {
            //Continue
        }
    }

    public static bool ShowMenu()
    {
        Console.Clear();
        foreach (var line in File.ReadAllLines("logo.txt"))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine("");
        Console.WriteLine("1 - Patient toevoegen");
        Console.WriteLine("2 - Patienten verwijderen");
        Console.WriteLine("3 - Arts toevoegen");
        Console.WriteLine("4 - Arts verwijderen");
        Console.WriteLine("5 - Afspraak toevoegen");
        Console.WriteLine("6 - Afspraken inzien");
        Console.WriteLine("7 - Sluiten");
        Console.WriteLine("8 - Reset db");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    AddPatient();
                    return true;
                case 2:
                    DeletePatient();
                    return true;
                case 3:
                    AddPhysician();
                    return true;
                case 4:
                    DeletePhysician();
                    return true;
                case 5:
                    AddAppointment();
                    return true;
                case 6:
                    ShowAppointment();
                    return true;
                case 7:
                    return false;
                case 8:
                    //EPDDbContext dbContext = new EPDDbContext();
                    //dbContext.Database.EnsureDeleted();
                    //dbContext.Database.EnsureCreated();
                    return true;
                default:
                    return true;
            }
        }

        return true;
    }

    #endregion
}