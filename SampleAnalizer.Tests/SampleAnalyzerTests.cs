using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SampleAnalizer.Tests
{
    public class SampleAnalyzerTests : CSharpAnalyzerTest<StaticFieldAnalizer, DefaultVerifier>
    {
        public SampleAnalyzerTests()
        {
            TestState.AdditionalReferences.Add(typeof(StaticField).Assembly);
        }

        [Fact]
        public async Task Should_Report_Diagnosticts()
        {
            TestCode = """
                using SampleAnalizer;

                static class Db
                {
                    public static void Select([StaticField] object selector)
                    {
                    }
                }

                class A 
                {
                    void Do()
                    {
                        Db.Select([|(object x) => x|]);
                    }
                }
            """;
            
            await RunAsync();
        }
    }
}
