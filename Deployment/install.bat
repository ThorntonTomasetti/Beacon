ECHO OFF
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2018" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2018\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2018\Beacon.addin"
	md "C:\ProgramData\Autodesk\Revit\Addins\2018\Beacon"
	copy /y Revit2018 C:\ProgramData\Autodesk\Revit\Addins\2018
	copy /y Revit2018\Beacon C:\ProgramData\Autodesk\Revit\Addins\2018\Beacon
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2019" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2019\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2019\Beacon.addin"
	md "C:\ProgramData\Autodesk\Revit\Addins\2019\Beacon"
	copy /y Revit2019 C:\ProgramData\Autodesk\Revit\Addins\2019
	copy /y Revit2019\Beacon C:\ProgramData\Autodesk\Revit\Addins\2019\Beacon
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2020" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2020\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2020\Beacon.addin"
	md "C:\ProgramData\Autodesk\Revit\Addins\2020\Beacon"
	copy /y Revit2020 C:\ProgramData\Autodesk\Revit\Addins\2020
	copy /y Revit2020\Beacon C:\ProgramData\Autodesk\Revit\Addins\2020\Beacon
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2021" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2021\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2021\Beacon.addin"
	md "C:\ProgramData\Autodesk\Revit\Addins\2021\Beacon"
	copy /y Revit2021 C:\ProgramData\Autodesk\Revit\Addins\2021
	copy /y Revit2021\Beacon C:\ProgramData\Autodesk\Revit\Addins\2021\Beacon
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2022" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2022\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2022\Beacon.addin"
	md "C:\ProgramData\Autodesk\Revit\Addins\2022\Beacon"
	copy /y Revit2022 C:\ProgramData\Autodesk\Revit\Addins\2022
	copy /y Revit2022\Beacon C:\ProgramData\Autodesk\Revit\Addins\2022\Beacon
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2023" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2023\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2023\Beacon.addin"
	md "C:\ProgramData\Autodesk\Revit\Addins\2023\Beacon"
	copy /y Revit2023 C:\ProgramData\Autodesk\Revit\Addins\2023
	copy /y Revit2023\Beacon C:\ProgramData\Autodesk\Revit\Addins\2023\Beacon
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2024" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2024\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2024\Beacon.addin"
	md "C:\ProgramData\Autodesk\Revit\Addins\2024\Beacon"
	copy /y Revit2024 C:\ProgramData\Autodesk\Revit\Addins\2024
	copy /y Revit2024\Beacon C:\ProgramData\Autodesk\Revit\Addins\2024\Beacon
)
pause