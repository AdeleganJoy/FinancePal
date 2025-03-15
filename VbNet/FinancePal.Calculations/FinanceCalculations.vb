Namespace FinancePal.Calculations
    Public Class FinanceCal
        Public Function CalcBalance(curr_balance As Decimal, amount As Decimal, category As String) As Decimal
            If category.ToLower() = "salary" Then
                Return curr_balance + amount
            Else
                Return curr_balance - amount
            End If
        End Function
    End Class
End Namespace
