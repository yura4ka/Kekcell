using Kekcell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Principal;

namespace Tests
{
    [TestClass]
    public class TableTests
    {
        [TestMethod]
        public void Input_BigExpression()
        {
            var table = new Table(5, 5);
            table.UpdateCellValue(0, 0, "1");
            table.UpdateCellValue(0, 1, "2");
            table.UpdateCellValue(0, 2, "3");
            table.UpdateCellValue(0, 3, "4");
            table.UpdateCellValue(0, 4, "5");
            table.UpdateCellExpression(1, 0, "Sum(A1, B1, C1, D1, E1) * 4 * Sin(30deg)");
            Assert.AreEqual("30", table.Data.Rows[1][0], "Value is not correct!");
        }

        [TestMethod]
        public void Input_Expressions_ChangeValue()
        {
            var table = new Table(5, 5);
            table.UpdateCellExpression(0, 0, "10");
            table.UpdateCellExpression(0, 1, "A1 * 2");
            table.UpdateCellExpression(0, 2, "B1 * 3");
            table.UpdateCellValue(0, 0, "3");

            Assert.AreEqual("6", table.Data.Rows[0][1], "Value is not correct!");
            Assert.AreEqual("18", table.Data.Rows[0][2], "Value is not correct!");
        }

        [TestMethod]
        public void Input_DevideByZero_ShouldThrowCellException()
        {
            var table = new Table(5, 5);
            table.UpdateCellExpression(0, 0, "10");
            Assert.ThrowsException<CellZeroDivisionException>(
                () => table.UpdateCellExpression(0, 1, "A1 / 0"));
        }
    }
}
