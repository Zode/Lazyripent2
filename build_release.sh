#!/bin/bash
dotnet test Lazyripent2.Tests/Lazyripent2.Tests.csproj
if [ $? -eq 0 ]; then
	echo 'Tests succeeded'
else
	echo 'Tests failed'
	exit 1
fi

mkdir -p Release
if [ -f Release/lazyripent.7z ]; then
	rm Release/lazyripent.7z
fi

if [ -f Release/lazyripent ]; then
	rm Release/lazyripent
fi

if [ -f Release/lazyripent.exe ]; then
	rm Release/lazyripent.exe
fi

function Build()
{
	echo "Building $1"
	dotnet publish Lazyripent2/Lazyripent2.csproj  --artifacts-path Build/"$1"Artifacts/ -o Build/$1 -c Release -r $2 -v q

	if [ $? -eq 0 ]; then
		echo "$1 build succeeded"
		echo '7-zipping'
		if [ "$1" = "Linux" ]; then
			fileExt=
		else
			fileExt=.exe
		fi

		mv Build/$1/Lazyripent2$fileExt Release/lazyripent$fileExt
		7z a Release/lazyripent.7z Release/lazyripent$fileExt -y -bso0
		7z rn Release/lazyripent.7z Release/lazyripent$fileExt lazyripent$fileExt -y -bso0
	else
		echo "$1 build failed"
		exit 1
	fi
}

Build "Linux" "linux-x64"
Build "Windows" "win-x64"
echo 'SHIP IT!!'