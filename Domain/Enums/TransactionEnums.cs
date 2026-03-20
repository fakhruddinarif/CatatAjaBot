namespace CatatAja.Domain.Enums;

public enum TransactionType
{
    Income,
    Expense,
    Transfer
};

public enum CategoryType
{
    Income,
    Expense
}

public enum DebtType
{
    Payable,    // Hutang (Kita meminjam uang)
    Receivable  // Piutang (Uang kita dipinjam orang)
}

public enum DebtStatus
{
    Unpaid,
    Partial,
    Paid
}