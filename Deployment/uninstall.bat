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
pause