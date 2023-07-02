#!/usr/bin/env bash
set -e

## install latest .NET 7.0 release
pushd /tmp
wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
chmod u+x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 7.0
popd

## publish project to known location for subsequent deployment by Netlify
dotnet build FFMQWebAsm --configuration Release && dotnet publish FFMQWebAsm -c Release --no-build -o publishoutput