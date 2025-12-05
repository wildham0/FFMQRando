#!/usr/bin/env bash
set -e

## install latest .NET 9.0 release
#pushd /tmp
#wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
#wget https://dot.net/v1/dotnet-install.sh
#chmod u+x /tmp/dotnet-install.sh
#/tmp/dotnet-install.sh --channel 9.0
#popd
apt install dotnet-sdk-9.0

## publish project to known location for subsequent deployment by Netlify
dotnet build FFMQWebAsm --configuration Release && dotnet publish FFMQWebAsm -c Release --no-build -o publishoutput