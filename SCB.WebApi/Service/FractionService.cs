using System.Runtime.Versioning;

public class FractionService
{
    /// <summary>
    /// Divides the first fraction by the second fraction.
    /// </summary>
    /// <param name="f1">The dividend fraction.</param>
    /// <param name="f2">The divisor fraction.</param>
    /// <returns>The result of dividing <paramref name="f1"/> by <paramref name="f2"/> as a simplified fraction.</returns>
    public Fraction Division(Fraction f1, Fraction f2)
    {
        Fraction dividedFraction = new Fraction
        {
            Teller = f1.Teller * f2.Noemer,
            Noemer = f1.Noemer * f2.Teller
        };
        return SimplifyFraction(dividedFraction);
    }
    /// <summary>
    /// Multiplies the first and second fraction together.
    /// </summary>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    /// <returns>The product of the two fractions simplified</returns>
    public Fraction Multiplication(Fraction f1, Fraction f2)
    {
        Fraction multipliedFraction = new Fraction
        {
            Teller = f1.Teller * f2.Teller,
            Noemer = f1.Noemer * f2.Noemer
        };
        return SimplifyFraction(multipliedFraction);
    }
    /// <summary>
    /// Adds the first and second fraction together
    /// </summary>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    /// <returns>The sum of the two fractions simplified</returns>
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
    /// <summary>
    /// Subtracts the second fraction from the first
    /// </summary>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    /// <returns>The difference of the two fractions simplified</returns>
    public Fraction Subtraction(Fraction f1, Fraction f2)
    {
        EvenNoemer(f1, f2);
        Fraction afterSubtraction = new Fraction
        {
            Teller = f1.Teller - f2.Teller,
            Noemer = f1.Noemer
        };
        return SimplifyFraction(afterSubtraction);
    }
    /// <summary>
    /// Evens the denominator of two fractions
    /// </summary>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    public void EvenNoemer(Fraction f1, Fraction f2)
    {
        int noemer = f1.Noemer * f2.Noemer;
        f1.Teller *= f2.Noemer;
        f2.Teller *= f1.Noemer;
        f1.Noemer = noemer;
        f2.Noemer = noemer;
    }
    /// <summary>
    /// Simplifies a simple fraction into the smallest possible denominator
    /// </summary>
    /// <param name="fraction"></param>
    /// <returns>The input fraction with smallest possible denominator</returns>
    public Fraction SimplifyFraction(Fraction fraction)
    {
        int a, b, temp;

        if (fraction.Teller == fraction.Noemer)
        {
            return new Fraction
            {
                Teller = 1,
                Noemer = 1
            };
        }
        else if (fraction.Noemer > fraction.Teller)
        {
            a = fraction.Noemer;
            b = fraction.Teller;
        }
        else
        {
            a = fraction.Teller;
            b = fraction.Noemer;
        }

        while (b != 0)
        {
            temp = b;
            b = a % b;
            a = temp;
        }

        fraction.Teller /= a;
        fraction.Noemer /= a;

        return fraction;
    }

    public FractionCompressed DecompressFraction(Fraction fraction)
    {
        if (fraction.Teller < fraction.Noemer)
        {
            return new FractionCompressed
            {
                Integer = 0,
                Fraction = new Fraction
                {
                    Teller = fraction.Teller,
                    Noemer = fraction.Noemer
                }
            };
        }

        if (fraction.Teller == fraction.Noemer)
        {
            return new FractionCompressed
            {
                Integer = 1,
                Fraction = new Fraction
                {
                    Teller = 0,
                    Noemer = 1
                }
            };
        }

        int newInteger = fraction.Teller / fraction.Noemer;

        return new FractionCompressed
        {
            Integer = newInteger,
            Fraction = new Fraction
            {
                Teller = fraction.Teller % fraction.Noemer,
                Noemer = fraction.Noemer
            }
        };
    }

}