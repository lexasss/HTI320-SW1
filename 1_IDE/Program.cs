Console.WriteLine("Pleae enter a number between 1 and 100:");

var input = Console.ReadLne();
int number = Convert.ToInt32(input);
Console.WriteLine($"You entered: {number}"); 

int squared = Square(number);
Console.WriteLine($"Squared number: {squared}");

static int Square(int n)
{
    return n + n;
}