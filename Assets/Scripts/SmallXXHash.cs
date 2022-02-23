public readonly struct SmallXXHash
{
    private const uint PrimeA = 0b10011110001101110111100110110001;
    private const uint PrimeB = 0b10000101111010111100101001110111;
    private const uint PrimeC = 0b11000010101100101010111000111101;
    private const uint PrimeD = 0b00100111110101001110101100101111;
    private const uint PrimeE = 0b00010110010101100110011110110001;

    private readonly uint _accumulator;

    public static implicit operator uint(SmallXXHash hash)
    {
        uint avalanche = hash._accumulator;
        avalanche ^= avalanche >> 15;
        avalanche *= PrimeB;
        avalanche ^= avalanche >> 13;
        avalanche *= PrimeC;
        avalanche ^= avalanche >> 16;
        return avalanche;
    }

    public static implicit operator SmallXXHash(uint value) => new SmallXXHash(value);

    public static SmallXXHash Seed(int seed) => (uint)seed * PrimeE;

    public SmallXXHash Eat(int data) => RotateLeft(_accumulator + (uint)data * PrimeC, 17) * PrimeD;

    public SmallXXHash Eat(byte data) => RotateLeft(_accumulator + data * PrimeE, 11) * PrimeA;

    private static uint RotateLeft(uint data, int steps) => (data << steps) | (data >> 32 - steps);

    private SmallXXHash(uint accumulator)
    {
        _accumulator = accumulator;
    }
}