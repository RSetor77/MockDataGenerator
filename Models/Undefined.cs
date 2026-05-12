namespace MockDataGenerator.Models
{
    public class Undefined
    {
        // Приватный конструктор запрещает создание копий
        private Undefined() { }

        // Единственный экземпляр в памяти
        public static Undefined Value { get; } = new Undefined();

        public override string ToString() => "[None/Undefined]";
    }
}
