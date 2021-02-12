# Rocket-Elevators-Csharp-Controller

* You can run the code with at the terminal of your preference by typing: **dotnet run**

    Note that you have to be in the script folder **Commercial_Controller_CS** for it to run correctly.

Testing: Public Class Commercial_Controller
Uncomment by removing // at the scenario1, scenario2, scenario3, scnenario4 to run the testing. 

SUMMARY:
1- class Battery
    a- Access: public, class name: Battery, instance variables, constructor declaration of class.
    b= Method tostring 
    c- Methods to create a list: createColumnsList, createListsInsideColumns
    d- Methods for logic: calculateamountOfFloorsPerColumn, etColumnValues, initializeBasementColumnFloors, initializeMultiColumnFloors, initializeUniqueColumnFloors
    
    
2- class Column
    a- Access: public, class name: Column, instance variables, constructor declaration of class.
    b= Method tostring 
    c- Methods to create a list: createFloorDoorsList, createButtonsUpList, createButtonsDownList
    d- Methods for logic: findElevator, findNearestElevator, manageButtonStatusOn
    e. Entry method: requestElevator
3- class Elevator
    a- Access: public, class name: Column, instance variables, constructor declaration of class.
    b= Method tostring 
    c- Methods to create a list: createFloorDoorsList, createDisplaysList, createfloorRequestButtonsList
    d- Methods for logic: moveElevator, moveUp, moveDown, manageButtonStatusOff, updateDisplays, openDoors, closeDoors, checkWeight, checkObstruction, addFloorTofloorRequestList, deleteFloorFromList
    e. Entry method: requestFloor
4- class Door
5- class Display
6- ENUMS: special class represents a group of constants 



