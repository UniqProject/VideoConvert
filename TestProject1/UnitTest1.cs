using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    using SharpDvdInfo;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            DvdInfoContainer dvdInfo = new DvdInfoContainer(@"G:\FilmeUnpack\XFILES_SEASON9_DISC5");
        }
    }
}
