namespace TidyHPC.LiteJson;
public partial struct Json
{
    /// <summary>
    /// 自增
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static Json operator ++(Json json)
    {
        if (json.IsNumber)
        {
            return json + 1;
        }
        return json;
    }

    /// <summary>
    /// 自减
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static Json operator --(Json json)
    {
        if (json.IsNumber)
        {
            return json - 1;
        }
        return json;
    }

    /// <inheritdoc/>
    public static bool operator ==(Json left, Json right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(Json left, Json right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Add
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Json operator +(Json left, Json right)
    {
        if (left.IsString && right.IsString)
        {
            return left.AsString + right.AsString;
        }
        else if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber + right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }

    /// <summary>
    /// Minus
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Json operator -(Json left, Json right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber - right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }

    /// <summary>
    /// Multiply
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Json operator *(Json left, Json right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber * right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }

    /// <summary>
    /// Divide
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Json operator /(Json left, Json right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber / right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }

    /// <summary>
    /// Greater than
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static bool operator >(Json left, Json right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber > right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }

    /// <summary>
    /// Less than
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static bool operator <(Json left, Json right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber < right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }

    /// <summary>
    /// Greater than or equal to
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static bool operator >=(Json left, Json right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber >= right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }

    /// <summary>
    /// Less than or equal to
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static bool operator <=(Json left, Json right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber <= right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }
    
    /// <summary>
    /// Modulus
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Json operator %(Json left, Json right)
    {
        if (left.IsNumber && right.IsNumber)
        {
            return left.AsNumber % right.AsNumber;
        }
        throw new Exception("Invalid operation");
    }
}
