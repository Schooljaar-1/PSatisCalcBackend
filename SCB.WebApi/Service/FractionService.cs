using System.Runtime.Versioning;

public class FractionService
{
    public Fraction Division(Fraction f1, Fraction f2)
    {
        Fraction divisedFraction = new Fraction
        {
            Teller = f1.Teller * f2.Noemer,
            Noemer = f1.Noemer * f2.Teller
        };
        return SimplifyFraction(divisedFraction);
    }

    public Fraction Multiplication(Fraction f1, Fraction f2)
    {
        Fraction multipliedFraction = new Fraction
        {
            Teller = f1.Teller * f2.Teller,
            Noemer = f1.Noemer * f2.Noemer
        };
        return SimplifyFraction(multipliedFraction);
    }

    public Fraction Addition(Fraction f1, Fraction f2)
    {
        EvenNoemer(f1, f2);
        Fraction afterAddition = new Fraction
        {
            Teller = f1.Teller + f2.Teller,
            Noemer = f1.Noemer
        };
        return SimplifyFraction(afterAddition);
    }

    public Fraction Subtraction (Fraction f1, Fraction f2)
    {
        EvenNoemer(f1, f2);
        Fraction afterSubtraction = new Fraction
        {
            Teller = f1.Teller - f2.Teller,
            Noemer = f1.Noemer
        };
        return SimplifyFraction(afterSubtraction);
    }

    public void EvenNoemer(Fraction f1, Fraction f2)
    {
        int noemer = f1.Noemer * f2.Noemer;
        f1.Teller *= f2.Noemer;
        f2.Teller *= f1.Noemer;
        f1.Noemer = noemer;
        f2.Noemer = noemer;
    }

    public Fraction SimplifyFraction(Fraction fraction)
    {
        int a, b, temp;

        if(fraction.Teller == fraction.Noemer)
        {
            return new Fraction
            {
                Teller = 1,
                Noemer = 1
            };
        }
        else if(fraction.Noemer > fraction.Teller)
        {
            a = fraction.Noemer;
            b = fraction.Teller;
        }
        else
        {
            a = fraction.Teller;
            b = fraction.Noemer;
        }

        while(b != 0)
        {
            temp = b;
            b = a % b;
            a = temp;
        }

        fraction.Teller /= a;
        fraction.Noemer /= a;

        return fraction;
    }
}