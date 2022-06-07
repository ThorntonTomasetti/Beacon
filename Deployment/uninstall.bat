ECHO OFF
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2018" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2018\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2018\Beacon.addin"
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2019" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2019\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2019\Beacon.addin"
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2020" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2020\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2020\Beacon.addin"
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2021" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2021\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2021\Beacon.addin"
)
IF EXIST "C:\ProgramData\Autodesk\Revit\Addins\2022" (
        rd /s /q "C:\ProgramData\Autodesk\Revit\Addins\2022\Beacon"
	del /q "C:\ProgramData\Autodesk\Revit\Addins\2022\Beacon.addin"
)
pause