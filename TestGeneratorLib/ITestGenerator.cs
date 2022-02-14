using System.Threading.Tasks;

namespace TestGeneratorLib
{
    public interface ITestGenerator
    {
        public Task<string> Generate(string code);
    }
}