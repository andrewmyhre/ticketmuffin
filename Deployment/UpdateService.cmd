set BuildFolder=%1
set ExecutableFilename=%2
set InstallFolder=%3
set ServiceName=%4

sc stop %ServiceName%
robocopy %BuildFolder% %InstallFolder% /mir
sc start %ServiceName%