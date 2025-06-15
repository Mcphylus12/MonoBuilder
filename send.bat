@echo off
git status
echo Press any key to submit changes
pause > nul
git add .
git commit -m %1
git push