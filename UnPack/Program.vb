'C:\Program Files\Google\Chrome\Application\125.0.6422.142

Imports System
Imports System.Buffers
Imports System.IO


Module Program

    Public br As BinaryReader
    Public input As String

    Sub Main(args As String())
        If args.Count = 0 Then
            Console.WriteLine("Tool UnPack - 2CongLC.vn :: 2024")
        Else
            input = args(0)
        End If
        Dim p As String = Nothing
        If IO.File.Exists(input) Then

            br = New BinaryReader(File.OpenRead(input))

            Dim version As UInteger = br.ReadUInt32()
            Dim encoding As Byte = br.ReadByte()
            br.BaseStream.Seek(3, SeekOrigin.Current)
            Dim resourceCount As UShort = br.ReadUInt16()
            Dim aliasCount As UShort = br.ReadUInt16()

            Dim entries As Entry() = New Entry(resourceCount + 1 - 1) {}
            Dim aliases As Aliass() = New Aliass(aliasCount - 1) {}

            Console.WriteLine(New With {.version = version, .encoding = encoding, .resourceCount = resourceCount, .aliasCount = aliasCount})

            For i As Integer = 0 To resourceCount
                Dim resourceId As UShort = br.ReadUInt16()
                Dim fileOffset As UInteger = br.ReadUInt32()
                entries(i) = New Entry(resourceId, fileOffset)
                Console.WriteLine(New With {.resourceId = resourceId, .fileOffset = fileOffset})
            Next

            For i As Integer = 0 To aliasCount - 1
                Dim resourceId As UShort = br.ReadUInt16()
                Dim entryIndex As UShort = br.ReadUInt16()
                aliases(i) = New Aliass(resourceId, entryIndex)
                Console.WriteLine(New With {.resourceId = resourceId, .entryIndex = entryIndex})
            Next

            p = Path.GetDirectoryName(input) & "\" & Path.GetFileNameWithoutExtension(input)
            Directory.CreateDirectory(p)

            For i As Integer = 0 To resourceCount - 1
                br.BaseStream.Seek(entries(i).FileOffset, SeekOrigin.Begin)
                Dim length As Integer = CInt(entries(i + 1).FileOffset - entries(i).FileOffset)
                Dim buff As Byte() = ArrayPool(Of Byte).Shared.Rent(length)
                Dim bytesRead As Integer = br.Read(buff, 0, length)

                Using bw As New BinaryWriter(File.Create(Path.Combine(p, $"f_{entries(i).ResourceId:X4}")))
                    bw.Write(buff, 0, length)
                End Using

                ArrayPool(Of Byte).Shared.Return(buff)
            Next

            br.Close()
            Console.WriteLine("unpack done!!!")
        End If
        Console.ReadLine()
    End Sub



    Public Structure Entry
        Public ReadOnly ResourceId As UShort
        Public ReadOnly FileOffset As UInteger

        Public Sub New(resourceId As UShort, fileOffset As UInteger)
            Me.ResourceId = resourceId
            Me.FileOffset = fileOffset
        End Sub
    End Structure

    Public Structure Aliass
        Public ReadOnly ResourceId As UShort
        Public ReadOnly EntryIndex As UShort

        Public Sub New(resourceId As UShort, entryIndex As UShort)
            Me.ResourceId = resourceId
            Me.EntryIndex = entryIndex
        End Sub
    End Structure

End Module
