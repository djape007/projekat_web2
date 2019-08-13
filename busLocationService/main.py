import math
import random
import requests
import time
import json
import itertools

R = 6356000 #precnik zemlje u metrima, tako da su sva rastojanja u metrima
baseUrl = "http://localhost:52295/api/"

#kod ovih linija je sekvenca tacaka pogresno napisana
sortReverse = []

class GlobeNavigation():
    #svi proracuni su u radijanima!
    @staticmethod
    def PointToRad(p):
        return (math.radians(p[0]), math.radians(p[1]))

    @staticmethod
    def PointToDeg(p):
        return (math.degrees(p[0]), math.degrees(p[1]))

    @staticmethod
    def PathToDeg(path):
        return [GlobeNavigation.PointToDeg(p) for p in path]

    @staticmethod
    def PathToRad(path):
        return [GlobeNavigation.PointToRad(p) for p in path]

    @staticmethod
    def Distance(p1,p2):
        a = 0.5 - math.cos((p2[0] - p1[0]))/2 + math.cos(p1[0]) * math.cos(p2[0]) * (1 - math.cos((p2[1] - p1[1]))) / 2
        return 2 * R * math.asin(math.sqrt(a))

    @staticmethod
    def CalculateBearing(p1Rad,p2Rad):
        A = math.cos(p2Rad[0]) * math.sin(p2Rad[1] - p1Rad[1])
        B = (math.cos(p1Rad[0]) * math.sin(p2Rad[0])) - (math.sin(p1Rad[0]) * math.cos(p2Rad[0]) * math.cos(p2Rad[1] - p1Rad[1]))
        bearing = math.atan2(A,B)
        return bearing

    @staticmethod
    def MovePointAlongAngle(pRad, bearingRad, distance):
        Ad = distance / R
        la2 =  math.asin(math.sin(pRad[0]) * math.cos(Ad) + math.cos(pRad[0]) * math.sin(Ad) * math.cos(bearingRad))
        lo2 = pRad[1] + math.atan2(math.sin(bearingRad) * math.sin(Ad) * math.cos(pRad[0]), math.cos(Ad) - math.sin(pRad[0]) * math.sin(la2))
        #vrati novu tacku
        return (la2,lo2)

    @staticmethod
    def Move(pStart,pEnd, stepInMeters, precisionInMeters):
        if precisionInMeters < stepInMeters:
            return None

        while GlobeNavigation.Distance(pStart, pEnd) > precisionInMeters:
            bearing = GlobeNavigation.CalculateBearing(pStart, pEnd)
            pStart = GlobeNavigation.MovePointAlongAngle(pStart, bearing, stepInMeters)
            yield pStart

    @staticmethod
    def MoveAlongRoute(path, stepInMeters):
        precisionInMeters = stepInMeters + 0.01
        if len(path) < 2:
            print("putanja nema dovoljno tacaka :(")
            return None

        pStart = path[0]
        for i in range(len(path) - 1):
            pEnd = path[i + 1]

            if GlobeNavigation.Distance(pStart, pEnd) < stepInMeters:
                continue
            
            for pStart in GlobeNavigation.Move(pStart, pEnd, stepInMeters, precisionInMeters):
                yield pStart

class BusStop():
    busStopCache = {}
    X = 1
    Y = 1
    naziv = ""

    def __init__(self, naziv,X,Y):
        self.X = X
        self.Y = Y
        self.naziv = naziv

    def GetPosition(self):
        return (self.X,self.Y)

    @staticmethod
    def BusStopsOnLinesToBusStop(BusStopsOnLinesList):
        return [BusStop.GetBusStopFromServer(busStopOnLine['BusStopId']) for busStopOnLine in BusStopsOnLinesList]

    @staticmethod
    def GetBusStopFromServer(busStopId):
        r = requests.get(baseUrl + "BusStops/" + busStopId)

        if r.status_code == 200:
            data = r.json()
            BusStop.busStopCache[busStopId] = BusStop(data['Name'], math.radians(data['X']), math.radians(data['Y']))
            return BusStop.busStopCache[busStopId]
        else:
            print("Nisam uspeo da GET busstop: HTTP " + str(r.status_code))
            return None
    @staticmethod
    def Get(busStopId):
        if busStopId in BusStop.busStopCache:
            return BusStop.busStopCache[busStopId]
        else:
            return BusStop.GetBusStopFromServer(busStopId)

class Bus():
    allBusesCache = {}
    X = 1 #u radijanima
    Y = 1 #u radijanima
    Id = ""
    path = None
    iterator = None
    OnLine = ""
    firstStart = True
    speed = 1 #km/h
    updateBusPositionInterval = 0.5 #sec
    timer = 0
    lastBusStop = None
    minTimeOnBusStop = 5
    maxTimeOnBusStop = 13
    waitTimeOnNextBusStop = -1

    def __init__(self, Id,X,Y,OnLine):
        self.X = X
        self.Y = Y
        self.Id = Id
        self.OnLine = OnLine
        self.firstStart = True
        Bus.allBusesCache[Id] = self

    def SetPosition(self,point):
        self.X = point[0]
        self.Y = point[1]
    
    def SetPath(self,path):
        if len(path) == 0:
            self.path = None
            return

        self.path = path
        #postavim pocetnu poziciju autobusa (na pocetak linije)
        self.X = self.path[0][0]
        self.Y = self.path[0][1]

    def ChangeDirection(self):
        staraLinija = self.OnLine
        smer = self.OnLine[-1]
        if smer == "A":
            smer = "B"
        else:
            smer = "A"
        
        self.OnLine = self.OnLine[:-1] + smer
        print(self.Id + " dosao sam do kraja linije "+staraLinija+", sad se vracam nazad linijom " + self.OnLine)

    def Move(self):
        if self.path is None:
            yield None

        if self.iterator is None:
            pocetnaTacka = 0
            if self.firstStart:
                random.seed(str(time.time()) + self.Id + self.Id) 
                #bus krece sa pocetka putanje, da bi krenuo sa random tacke
                #samo odsecem listu na random mestu i bus krece odatle
                pocetnaTacka = random.randint(0, len(self.path) - 4)
                self.firstStart = False
            
            stepLength = (Bus.speed * 1000 / 3600) / Bus.updateBusPositionInterval #u metrima koliko ce autobus preci tokom jednog intervala updateovanja
            self.iterator = GlobeNavigation.MoveAlongRoute(self.path[pocetnaTacka:], stepLength)
        
        busStop = self.CheckForBusStop()

        if not busStop is None:
            if self.waitTimeOnNextBusStop == -1:
                self.waitTimeOnNextBusStop = random.randint(Bus.minTimeOnBusStop, Bus.maxTimeOnBusStop)

            while self.timer < self.waitTimeOnNextBusStop:
                self.timer += self.updateBusPositionInterval
                self.SetPosition(busStop.GetPosition())
                yield self.GetPosition()
            
            self.waitTimeOnNextBusStop = -1
            self.lastBusStop = busStop
            self.timer = 0
            newPosition = self.GetPosition()
        else:
            newPosition = next(self.iterator, None)

        if newPosition is None:
            self.iterator = None
            self.ChangeDirection()
            self.path = Line.GetLine(self.OnLine).path
            yield self.GetPosition()
        else:
            self.SetPosition(newPosition)
            yield self.GetPosition()

    def CheckForBusStop(self):
        #simulacija zaustavljanja autobusa na stanici
        for stanica in Line.allLinesCache[self.OnLine].busStops:
            if GlobeNavigation.Distance(self.GetPosition(), stanica.GetPosition()) <= 16:
                if stanica == self.lastBusStop:
                    return None
                return stanica
        
        return None

    def GetPosition(self):
        return (self.X, self.Y)
    
    def __str__(self):
        return self.Id + "," + str(math.degrees(self.X)) + "," + str(math.degrees(self.Y)) + "," + self.OnLine

    @staticmethod
    def FromWebSiteToList(listOfWBBuses):
        return [Bus(bus['Id'], math.radians(bus['X']), math.radians(bus['Y']), bus['LineId']) for bus in listOfWBBuses]

    @staticmethod
    def SendBusesToServer(listOfBuses):
        data = "|".join([str(bus) for bus in listOfBuses])
        r = requests.post(baseUrl + 'Buses/UpdatePosition', data=data)
        if r.status_code == 200:
            print("Nove pozicije poslate serveru (buseva poslato:" + str(len(listOfBuses)) + ")")
        else:
            print("Greska prilikom updateovanja pozicija buseva: HTTP " + str(r.status_code))
    
    @staticmethod
    def GetBus(busId):
        if busId in Bus.allBusesCache:
            return Bus.allBusesCache[busId]
        return None

class Line():
    allLinesCache = {}
    path = None
    busStops = None
    buses = None
    LineId = None

    def __init__(self,lineId,path,busStops,buses):
        self.path = path
        self.busStops = busStops
        self.LineId = lineId
        self.buses = buses

    @staticmethod
    def GetLineFromServer(lineId:str):
        if len(lineId) == 0:
            return None
        
        r = requests.get(baseUrl + "Lines/" + lineId)

        if r.status_code == 200:
            l = Line.LineWebSiteToLocalLine(r.json())
            Line.allLinesCache[lineId] = l
            return l
        else:
            print("Nisam uspeo da GETujem liniju: HTTP " + str(r.status_code))
            return None
    
    @staticmethod
    def LineWebSiteToLocalLine(data):
        l = Line(
            data['Id'], 
            Line.ConvertBusLinePathDBToPath(data['PointLinePaths'], data['Id']), 
            BusStop.BusStopsOnLinesToBusStop(data['BusStopsOnLines']),
            Bus.FromWebSiteToList(data['Buses'])
        )
        return l

    @staticmethod
    def ConvertBusLinePathDBToPath(busLinePathDB, LineId):
        #nema garancije da su tacke putanje sortirane, pa ih ovde sortiram
        if LineId in sortReverse:
            busLinePathDB.sort(key=lambda x: x['SequenceNumber'], reverse=True)
        else:
            busLinePathDB.sort(key=lambda x: x['SequenceNumber'], reverse=False)

        path = [GlobeNavigation.PointToRad((t['X'], t['Y'])) for t in busLinePathDB]
        return path

    @staticmethod
    def GetAllLinesFromServer():
        print("Trazim od servera sve linije")
        print("Ovo ce potrajati (minut-dva-tri)....")
        r = requests.get(baseUrl + 'Lines')

        if r.status_code == 200:
            listaLinija = [Line.LineWebSiteToLocalLine(linija) for linija in r.json()]
            dictLinija = {}
            for l in listaLinija:
                dictLinija[l.LineId] = l
            Line.allLinesCache = dictLinija
            return dictLinija
        else:
            print("Nisam uspeo da GETujem sve linije: HTTP " + str(r.status_code))
            return None
    
    @staticmethod
    def GetAllLinesWithBusesFromServer():
        print("Trazim od servera sve linije koje imaju bar jedan autobus")
        print("Ovo ce potrajati (minut-dva-tri)....")
        r = requests.get(baseUrl + 'Lines/WithBuses')

        if r.status_code == 200:
            listaLinija = [Line.LineWebSiteToLocalLine(linija) for linija in r.json()]
            dictLinija = {}
            for l in listaLinija:
                dictLinija[l.LineId] = l
            Line.allLinesCache = dictLinija
            return dictLinija
        else:
            print("Nisam uspeo da GETujem sve linije: HTTP " + str(r.status_code))
            return None
    @staticmethod
    def GetLine(lineId):
        if lineId in Line.allLinesCache:
            return Line.allLinesCache[lineId]
        else:
            return Line.GetLineFromServer(lineId)

def LoadConfig():
    try:
        with open('config.json', 'rb') as f:
            options = json.loads(f.read().decode('utf-8'))
            return options
    except Exception as e:
        print(str(e))
        print("Nema config.json fajla")
        return None

def Main():
    #parametri simuliranja kretanja buseva
    simulationSpeed = 1 #ovo povecaj ako hoces da se busevi krecu brze po mapi
    busSpeed = 50 #km/h
    sendDataToServerInterval = 1 #sec

    #######################################################################
    #######################################################################
    #ispod ne menjati nista
    updateBusPositionInterval = 0.1 #sec, povecanjem ovoga se smanjuje preciznost buseva da se krecu tacno po liniji

    getLines = []
    config = LoadConfig()
    if not config is None:
        print("config.json je ucitan")
        simulationSpeed = config['simulationSpeed']
        busSpeed = config['busSpeed']
        sendDataToServerInterval = config['updateServerInterval']
        global baseUrl
        baseUrl = config['baseUrl']
        global sortReverse
        sortReverse = config['reverseLines']
        getLines = config['simulateLines']
        Bus.minTimeOnBusStop = config['minBusStopTime']
        Bus.maxTimeOnBusStop = config['maxBusStopTime']

    Bus.updateBusPositionInterval = updateBusPositionInterval
    updateBusPositionInterval *= (1 / simulationSpeed)
    
    if len(getLines) > 0:
        print("Radim simulaciju za sledece linije: " + ",".join(getLines))
        for lineCode in getLines:
            Line.GetLine(lineCode)
    else:
        Line.GetAllLinesWithBusesFromServer()

    print("Linije su preuzete, incijalizacija putanja")

    #postavim svakom busu putanju po kojoj ce da vozi
    for line in list(Line.allLinesCache.values()):
        for bus in line.buses:
            bus.SetPath(line.path)
    print("Simulacija pocinje :D")
    timer = 0

    #glavni loop simulacije
    while True:
        for bus in list(Bus.allBusesCache.values()):
            p = next(bus.Move(), None)

            #namesteni su da idu u krug
            #kad dodju do kraja linije 22A predju na pocetak linije 22B
            #tako da nema potrebe za ovim ifom, al mozda nekad zatreba za debug
            if p is None:
                print(bus.Id + " je stigao do kraja")
            else:
                #print(bus.Id + " nova poz " + str(GlobeNavigation.PointToDeg(p)))
                pass

        time.sleep(updateBusPositionInterval)
        timer += updateBusPositionInterval

        if timer >= sendDataToServerInterval:
            Bus.SendBusesToServer(Bus.allBusesCache.values())
            timer = 0

if __name__ == "__main__":
    Main()