#!/bin/sh

newPath=`echo $0 | awk '{split($0, a, ";"); split(a[1], b, "/"); for(x = 2; x < length(b); x++){printf("/%s", b[x]);} print "";}'`
cd "$newPath"

../../PlanetUnity/.Support/Tools/gaxb csharp ./LD30Game.xsd -t ../../PlanetUnity/.Support/Tools/gaxb.templates -o ../

