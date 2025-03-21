Imports System.Math

Public Module _Calculate

    Public Function PercentageMarkup(ByVal valueAs100Percent As Double, ByVal valueAsXPercent As Double, ByVal round2decimals As Integer) As Double
        PercentageMarkup = 0
        Try
            '' caclulate percentage between BaseCost(valueAs100Percent) and SellPrice(valueAsXPercent):
            If 0 < valueAs100Percent Then
                PercentageMarkup = Math.Round((valueAsXPercent * 100 / valueAs100Percent - 100), round2decimals)
                '_Debug.Print_("_Caculate.PercentageMarkup(): " & PercentageMarkup.ToString & "%")
            End If
            ''
        Catch ex As Exception : _Debug.Print_("_Caculate.PercentageMarkup(): " & ex.Message)
        End Try
        ''
    End Function

    Public Function PercentageMarkupValue(ByVal valueAs100Percent As Double, ByVal markupPercent As Double, ByVal round2decimals As Integer) As Double
        PercentageMarkupValue = 0
        Try
            '' caclulate markup value based on the initial value and percent:
            PercentageMarkupValue = Math.Round(valueAs100Percent + (valueAs100Percent * (markupPercent / 100)), round2decimals)
            '_Debug.Print_("_Caculate.PercentageMarkupValue(): " & Format(PercentageMarkupValue, "$#,##0.00"))
            ''
        Catch ex As Exception : _Debug.Print_("_Caculate.PercentageMarkupValue(): " & ex.Message)
        End Try
        ''
    End Function

End Module
