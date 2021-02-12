using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Commercial_Controller_CS
{
    //------------------------------------------- BATTERY CLASS -----------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------
    class Battery
    {
        public int id;
        public int amountOfColumns;
        public int minBuildingFloor;                  //Is equal to 1 OR equal the amountOfBasements if there is a basement
        public int maxBuildingFloor;                  //Is the last floor of the building
        public int amountOfFloors;                    //Floors of the building excluding the number of basements
        public int amountOfBasements;
        public int totalNumberOfFloors;               //amountOfFloors + Math.abs(amountOfBasements)
        public int amountOfElevators;
        public int numberOfFloorsPerColumn;
        public BatteryStatus status;
        public List<Column> columnsList;

        //----------------- Constructor and its attributes -----------------//
        public Battery(int batteryId, int batteryamountOfColumns, int batteryTotalNumberOfFloors, int batteryamountOfBasements, int batteryamountOfElevators, BatteryStatus batteryStatus)
        {
            id = batteryId;
            amountOfColumns = batteryamountOfColumns;
            totalNumberOfFloors = batteryTotalNumberOfFloors;
            amountOfBasements = batteryamountOfBasements;
            amountOfElevators = batteryamountOfElevators;
            status = batteryStatus;
            columnsList = new List<Column>();
            numberOfFloorsPerColumn = calculateNumberOfFloorsPerColumn();
            createColumnsList();
            setColumnValues();
            createListsInsideColumns();
        }


        //----------------- Method toString -----------------//
        /* ******* GET A STRING REPRESENTATION OF BATTERY OBJECT ******* */
        public override string ToString()
        {
            return "battery" + this.id + " | Basements: " + this.amountOfBasements + " | Columns: " + this.amountOfColumns + " | Elevators per column: " + this.amountOfElevators;
        }


        //----------------- Methods to create a list -----------------//
        /* ******* CREATE A LIST OF COLUMNS FOR THE BATTERY ******* */
        public void createColumnsList()
        {
            char name = 'A';
            for (int i = 1; i <= this.amountOfColumns; i++)
            {
                this.columnsList.Add(new Column(i, name, Status.ACTIVE, this.amountOfElevators, numberOfFloorsPerColumn, amountOfBasements, this));
                // System.Console.WriteLine("column" + name + " created!!!");
                name = Convert.ToChar(name + 1);
            }
        }

        /* ******* CALL FUNCTIONS TO CREATE THE LISTS INSIDE EACH COLUMN ******* */
        public void createListsInsideColumns()
        {
            foreach (Column column in columnsList)
            {
                column.createElevatorsList();
                column.createButtonsUpList();
                column.createButtonsDownList();
            }
        }


        //----------------- Methods for logic -----------------//
        /* ******* LOGIC TO FIND THE FLOORS SERVED PER EACH COLUMN ******* */
        public int calculateNumberOfFloorsPerColumn()
        {
            amountOfFloors = totalNumberOfFloors - amountOfBasements; //amountOfBasements is negative
            int numberOfFloorsPerColumn;

            if (this.amountOfBasements > 0)
            { //if there is basement floors
                numberOfFloorsPerColumn = (this.amountOfFloors / (this.amountOfColumns - 1)); //the first column serves the basement floors
            }
            else
            { //there is no basement
                numberOfFloorsPerColumn = (this.amountOfFloors / this.amountOfColumns);
            }

            return numberOfFloorsPerColumn;
        }

        /* ******* LOGIC TO FIND THE REMAINING FLOORS OF EACH COLUMN AND SET VALUES servedFloors, minFloors, maxFloors ******* */
        public void setColumnValues()
        {
            int remainingFloors;

            //calculating the remaining floors
            if (this.amountOfBasements > 0)
            { //if there are basement floors
                remainingFloors = this.amountOfFloors % (this.amountOfColumns - 1);
            }
            else
            { //there is no basement
                remainingFloors = this.amountOfFloors % this.amountOfColumns;
            }

            //setting the minFloor and maxFloor of each column            
            if (this.amountOfColumns == 1)
            { //if there is just one column, it serves all the floors of the building
                initializeUniqueColumnFloors();
            }
            else
            { //for more than 1 column
                initializeMultiColumnFloors();

                //adjusting the number of served floors of the columns if there are remaining floors
                if (remainingFloors != 0)
                { //if the remainingFloors is not zero, then it adds the remaining floors to the last column
                    this.columnsList[this.columnsList.Count - 1].maxFloor = this.columnsList[this.columnsList.Count - 1].minFloor + this.columnsList[this.columnsList.Count - 1].servedFloors;
                    this.columnsList[this.columnsList.Count - 1].servedFloors = numberOfFloorsPerColumn + remainingFloors;
                }
                //if there is a basement, then the first column will serve the basements + RDC
                if (this.amountOfBasements > 0)
                {
                    initializeBasementColumnFloors();
                }
            }
        }

        /* ******* LOGIC TO SET THE minFloor AND maxFloor FOR THE BASEMENT COLUMN ******* */
        private void initializeBasementColumnFloors()
        {
            this.columnsList[0].servedFloors = (this.amountOfBasements + 1); //+1 is the RDC
            this.columnsList[0].minFloor = amountOfBasements * -1; //the minFloor of basement is a negative number
            this.columnsList[0].maxFloor = 1; //1 is the RDC
        }

        /* ******* LOGIC TO SET THE minFloor AND maxFloor FOR ALL THE COLUMNS EXCLUDING BASEMENT COLUMN ******* */
        private void initializeMultiColumnFloors()
        {
            int minimumFloor = 1;
            for (int i = 1; i < this.columnsList.Count; i++)
            { //if its not the first column (because the first column serves the basements)
                if (i == 1)
                {
                    this.columnsList[i].servedFloors = numberOfFloorsPerColumn;
                }
                else
                {
                    this.columnsList[i].servedFloors = (numberOfFloorsPerColumn + 1); //Add 1 floor for the RDC/ground floor
                }
                this.columnsList[i].minFloor = minimumFloor;
                this.columnsList[i].maxFloor = this.columnsList[i].minFloor + (numberOfFloorsPerColumn - 1);
                minimumFloor = this.columnsList[i].maxFloor + 1; //setting the minimum floor for the next column
            }
        }

        /* ******* LOGIC TO SET THE minFloor AND maxFloor IF THERE IS JUST ONE COLUMN ******* */
        private void initializeUniqueColumnFloors()
        {
            int minimumFloor = 1;
            this.columnsList[0].servedFloors = totalNumberOfFloors;
            if (amountOfBasements > 0)
            { //if there is basement
                this.columnsList[0].minFloor = amountOfBasements;
            }
            else
            { //if there is NO basement
                this.columnsList[0].minFloor = minimumFloor;
                this.columnsList[0].maxFloor = amountOfFloors;
            }
        }


        //------------------------------------------- COLUMN CLASS ------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        public class Column
        {
            public int id;
            public char name;
            public Status status;
            public int amountOfElevators;
            public int minFloor;
            public int maxFloor;
            public int servedFloors;
            public int amountOfBasements;
            public Battery battery;
            public List<Elevator> elevatorsList;
            public List<Button> buttonsUpList;
            public List<Button> buttonsDownList;

            //----------------- Constructor and its attributes -----------------//
            public Column(int ID, char columnName, Status Status, int columnNumberOfElevators, int columnServedFloors, int isBasements, Battery columnBattery)
            {
                id = ID;
                name = columnName;
                status = Status;
                amountOfElevators = columnNumberOfElevators;
                servedFloors = columnServedFloors;
                amountOfBasements = isBasements * -1;
                battery = columnBattery;
                elevatorsList = new List<Elevator>();
                buttonsUpList = new List<Button>();
                buttonsDownList = new List<Button>();
            }


            //----------------- Method toString -----------------//
            /* ******* GET A STRING REPRESENTATION OF COLUMN OBJECT ******* */
            public override string ToString()
            {
                return "column" + this.name + " | Served floors: " + this.servedFloors + " | Min floor: " + this.minFloor + " | Max floor: " + this.maxFloor;
            }


            //----------------- Methods to create a list -----------------//
            /* ******* CREATE A LIST OF ELEVATORS FOR THE COLUMN ******* */
            public void createElevatorsList()
            {
                for (int i = 1; i <= this.amountOfElevators; i++)
                {
                    this.elevatorsList.Add(new Elevator(i, this.servedFloors, 1, ElevatorStatus.IDLE, SensorStatus.OFF, SensorStatus.OFF, this));
                }
            }

            /* ******* CREATE A LIST WITH UP BUTTONS FROM THE FIRST FLOOR TO THE LAST LAST BUT ONE FLOOR ******* */
            public void createButtonsUpList()
            {
                buttonsUpList.Add(new Button(1, ButtonStatus.OFF, 1));
                for (int i = minFloor; i < this.maxFloor; i++)
                {
                    this.buttonsUpList.Add(new Button(i, ButtonStatus.OFF, i));
                }
            }

            /* ******* CREATE A LIST WITH DOWN BUTTONS FROM THE SECOND FLOOR TO THE LAST FLOOR ******* */
            public void createButtonsDownList()
            {
                buttonsDownList.Add(new Button(1, ButtonStatus.OFF, 1));
                int minBuildingFloor;
                if (amountOfBasements > 0)
                {
                    minBuildingFloor = amountOfBasements * -1;
                }
                else
                {
                    minBuildingFloor = 1;
                }
                for (int i = (minBuildingFloor + 1); i <= this.maxFloor; i++)
                {
                    this.buttonsDownList.Add(new Button(i, ButtonStatus.OFF, i));
                }
            }


            //----------------- Methods for logic -----------------//            
            /* ******* LOGIC TO FIND THE BEST ELEVATOR WITH A PRIORITIZATION LOGIC ******* */
            public Elevator findElevator(int currentFloor, Direction direction)
            {
                Elevator bestElevator;
                List<Elevator> activeElevatorList = new List<Elevator>();
                List<Elevator> idleElevatorList = new List<Elevator>();
                List<Elevator> sameDirectionElevatorList = new List<Elevator>();
                this.elevatorsList.ForEach(elevator =>
                {
                    if (elevator.status != ElevatorStatus.IDLE)
                    {
                        //Verify if the request is on the elevators way, otherwise the elevator will just continue its way ignoring this call
                        if (elevator.status == ElevatorStatus.UP && elevator.floor <= currentFloor || elevator.status == ElevatorStatus.DOWN && elevator.floor >= currentFloor)
                        {
                            activeElevatorList.Add(elevator);
                        }
                    }
                    else
                    {
                        idleElevatorList.Add(elevator);
                    }
                });

                if (activeElevatorList.Count > 0)
                { //Create new list for elevators and filter by same direction that the request
                    sameDirectionElevatorList = activeElevatorList.Where(elevator => elevator.status.ToString().Equals(direction.ToString())).ToList();
                }

                if (sameDirectionElevatorList.Count > 0)
                {
                    bestElevator = this.findNearestElevator(currentFloor, sameDirectionElevatorList); // 1- Try to use an elevator that is moving and has the same direction
                }
                else if (idleElevatorList.Count > 0)
                {
                    bestElevator = this.findNearestElevator(currentFloor, idleElevatorList); // 2- Try to use an elevator that is IDLE
                }
                else
                {
                    bestElevator = this.findNearestElevator(currentFloor, activeElevatorList); // 3- As the last option, uses an elevator that is moving at the contrary direction
                }

                return bestElevator;
            }

            /* ******* LOGIC TO FIND THE NEAREST ELEVATOR ******* */
            public Elevator findNearestElevator(int currentFloor, List<Elevator> selectedList)
            {
                Elevator bestElevator = selectedList[0];
                int bestDistance = Math.Abs(selectedList[0].floor - currentFloor); //Math.abs() returns the absolute value of a number (always positive).
                foreach (Elevator elevator in selectedList)
                {
                    if (Math.Abs(elevator.floor - currentFloor) < bestDistance)
                    {
                        bestElevator = elevator;
                    }
                }
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                System.Console.WriteLine("\n-----------------------------------------------------");
                System.Console.WriteLine("   > > >> >>> ELEVATOR " + this.name + bestElevator.id + " WAS CALLED <<< << < <");
                System.Console.WriteLine("-----------------------------------------------------\n");
                Console.ResetColor();

                return bestElevator;
            }

            /* ******* LOGIC TO TURN ON THE BUTTONS FOR THE ASKED DIRECTION ******* */
            public void manageButtonStatusOn(int requestedFloor, Direction direction)
            {
                if (direction == Direction.UP)
                {
                    //find the UP button by ID
                    Button currentButton = this.buttonsUpList.FirstOrDefault(button => button.id == requestedFloor);
                    if (currentButton != null)
                    {
                        currentButton.status = ButtonStatus.ON;
                    }
                }
                else
                {
                    //find the DOWN button by ID
                    Button currentButton = this.buttonsDownList.FirstOrDefault(button => button.id == requestedFloor);
                    if (currentButton != null)
                    {
                        currentButton.status = ButtonStatus.ON;
                    }
                }
            }


            //----------------- Entry method -----------------//
            /* ******* ENTRY METHOD ******* */
            /* ******* REQUEST FOR AN ELEVATOR BY PRESSING THE UP OU DOWN BUTTON OUTSIDE THE ELEVATOR ******* */
            public void requestElevator(int requestedFloor, Direction direction)
            { // User goes to the specific column and press a button outside the elevator requesting for an elevator
                manageButtonStatusOn(requestedFloor, direction);
                //        System.Console.WriteLine(">> Someone request an elevator from floor <" + requestedFloor + "> and direction <" + direction + "> <<");
                Elevator bestElevator = this.findElevator(requestedFloor, direction);
                if (bestElevator.floor != requestedFloor)
                {
                    bestElevator.addFloorToFloorList(requestedFloor);
                    bestElevator.moveElevator(requestedFloor);
                }
            }

        }


        //------------------------------------------- ELEVATOR CLASS ----------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        public class Elevator
        {
            public int id;
            public int servedFloors;
            public int floor;
            public ElevatorStatus status;
            public SensorStatus weightSensorStatus;
            public SensorStatus obstructionSensorStatus;
            public Column column;
            public Door elevatorDoor;
            public Display elevatorDisplay;
            public List<Door> floorDoorsList;
            public List<Display> floorDisplaysList;
            public List<Button> floorRequestButtonsList;
            public List<int> floorRequestList;

            //----------------- Constructor and its attributes -----------------//
            public Elevator(int elevatorId, int elevatorservedFloors, int elevatorFloor, ElevatorStatus elevatorStatus, SensorStatus weightStatus, SensorStatus obstructionStatus, Column elevatorColumn)
            {
                id = elevatorId;
                servedFloors = elevatorservedFloors;
                floor = elevatorFloor;
                status = elevatorStatus;
                weightSensorStatus = weightStatus;
                obstructionSensorStatus = obstructionStatus;
                column = elevatorColumn;
                elevatorDoor = new Door(0, DoorStatus.CLOSED, 0);
                elevatorDisplay = new Display(0, DisplayStatus.ON, 0);
                floorDoorsList = new List<Door>();
                floorDisplaysList = new List<Display>();
                floorRequestButtonsList = new List<Button>();
                floorRequestList = new List<int>();

                this.createFloorDoorsList();
                this.createDisplaysList();
                this.createfloorRequestButtonsList();
            }

            //----------------- Method toString -----------------//
            /* ******* GET A STRING REPRESENTATION OF ELEVATOR OBJECT ******* */
            public override string ToString()
            {
                return "elevator" + column.name + this.id + " | Floor: " + this.floor + " | Status: " + this.status;
            }


            //----------------- Methods to create a list -----------------//
            /* ******* CREATE A LIST WITH A DOOR OF EACH FLOOR ******* */
            public void createFloorDoorsList()
            {
                floorDoorsList.Add(new Door(1, DoorStatus.CLOSED, 1));
                for (int i = column.minFloor; i <= this.column.maxFloor; i++)
                {
                    this.floorDoorsList.Add(new Door(i, DoorStatus.CLOSED, i));
                }
            }

            /* ******* CREATE A LIST WITH A DISPLAY OF EACH FLOOR ******* */
            public void createDisplaysList()
            {
                floorDisplaysList.Add(new Display(1, DisplayStatus.ON, 1));
                for (int i = column.minFloor; i <= this.column.maxFloor; i++)
                {
                    this.floorDisplaysList.Add(new Display(i, DisplayStatus.ON, i));
                }
            }

            /* ******* CREATE A LIST WITH A BUTTON OF EACH FLOOR ******* */
            public void createfloorRequestButtonsList()
            {
                floorRequestButtonsList.Add(new Button(1, ButtonStatus.OFF, 1));
                for (int i = column.minFloor; i <= this.column.maxFloor; i++)
                {
                    this.floorRequestButtonsList.Add(new Button(i, ButtonStatus.OFF, i));
                }
            }


            //----------------- Methods for logic -----------------//
            /* ******* LOGIC TO MOVE ELEVATOR ******* */
            public void moveElevator(int requestedFloor)
            {
                while (this.floorRequestList.Count > 0)
                {
                    if (this.status == ElevatorStatus.IDLE)
                    {
                        if (this.floor < requestedFloor)
                        {
                            this.status = ElevatorStatus.UP;
                        }
                        else if (this.floor > requestedFloor)
                        {
                            this.status = ElevatorStatus.DOWN;
                        }
                        else
                        {
                            this.openDoors();
                            this.deleteFloorFromList(requestedFloor);
                            this.manageButtonStatusOff(requestedFloor);

                        }
                    }
                    if (this.status == ElevatorStatus.UP)
                    {
                        this.moveUp();
                    }
                    else if (this.status == ElevatorStatus.DOWN)
                    {
                        this.moveDown();
                    }
                }
            }

            /* ******* LOGIC TO MOVE UP ******* */
            public void moveUp()
            {
                List<int> tempArray = new List<int>(this.floorRequestList);
                for (int i = this.floor; i < tempArray[tempArray.Count - 1]; i++)
                {
                    int j = i;
                    Door currentDoor = this.floorDoorsList.FirstOrDefault(door => door.id == j);
                    if (currentDoor != null && currentDoor.status == DoorStatus.OPENED || this.elevatorDoor.status == DoorStatus.OPENED)
                    {
                        System.Console.WriteLine("   Doors are open, closing doors before move up");
                        this.closeDoors();
                    }
                    System.Console.WriteLine("Moving elevator" + column.name + this.id + " <up> from floor " + i + " to floor " + (i + 1));
                    int nextFloor = (i + 1);
                    this.floor = nextFloor;
                    this.updateDisplays(this.floor);

                    if (tempArray.Contains(nextFloor))
                    {
                        this.openDoors();
                        this.deleteFloorFromList(nextFloor);
                        this.manageButtonStatusOff(nextFloor);
                    }
                }
                if (this.floorRequestList.Count == 0)
                {
                    this.status = ElevatorStatus.IDLE;
                    //    System.Console.WriteLine("       Elevator" + column.name + this.id + " is now " + this.status);
                }
                else
                {
                    this.status = ElevatorStatus.DOWN;
                    System.Console.WriteLine("       Elevator" + column.name + this.id + " is now going " + this.status);
                }
            }

            /* ******* LOGIC TO MOVE DOWN ******* */
            public void moveDown()
            {
                List<int> tempArray = new List<int>(this.floorRequestList);
                for (int i = this.floor; i > tempArray[tempArray.Count - 1]; i--)
                {

                    int j = i;
                    Door currentDoor = this.floorDoorsList.FirstOrDefault(door => door.id == j); // finding doors by id
                    if (currentDoor != null && currentDoor.status == DoorStatus.OPENED || this.elevatorDoor.status == DoorStatus.OPENED)
                    {
                        System.Console.WriteLine("       Doors are open, closing doors before move down");
                        this.closeDoors();
                    }

                    System.Console.WriteLine("Moving elevator" + column.name + this.id + " <down> from floor " + i + " to floor " + (i - 1));
                    int nextFloor = (i - 1);
                    this.floor = nextFloor;
                    this.updateDisplays(this.floor);

                    if (tempArray.Contains(nextFloor))
                    {
                        this.openDoors();
                        this.deleteFloorFromList(nextFloor);
                        this.manageButtonStatusOff(nextFloor);
                    }
                }
                if (this.floorRequestList.Count() == 0)
                {
                    this.status = ElevatorStatus.IDLE;
                    //    System.Console.WriteLine("       Elevator" + column.name + this.id + " is now " + this.status);
                }
                else
                {
                    this.status = ElevatorStatus.UP;
                    System.Console.WriteLine("       Elevator" + column.name + this.id + " is now going " + this.status);
                }
            }

            /* ******* LOGIC TO FIND BUTTONS BY ID AND SET BUTTON STATUS OFF ******* */
            private void manageButtonStatusOff(int floor)
            {
                Button currentUpButton = this.column.buttonsUpList.FirstOrDefault(button => button.id == floor);
                if (currentUpButton != null)
                {
                    currentUpButton.status = ButtonStatus.OFF;
                }
                Button currentDownButton = this.column.buttonsDownList.FirstOrDefault(button => button.id == floor);
                if (currentDownButton != null)
                {
                    currentDownButton.status = ButtonStatus.OFF;
                }
                Button currentFloorButton = this.floorRequestButtonsList.FirstOrDefault(button => button.id == floor);
                if (currentFloorButton != null)
                {
                    currentFloorButton.status = ButtonStatus.OFF;
                }
            }

            /* ******* LOGIC TO UPDATE DISPLAYS OF ELEVATOR AND SHOW FLOOR ******* */
            public void updateDisplays(int elevatorFloor)
            {
                this.floorDisplaysList.ForEach(display =>
                {
                    display.floor = elevatorFloor;
                });
                //    System.Console.WriteLine("Displays show #" + elevatorFloor);
            }

            /* ******* LOGIC TO OPEN DOORS ******* */
            public void openDoors()
            {
                System.Console.WriteLine("       Elevator is stopped at floor " + this.floor);
                System.Console.WriteLine("       Opening doors...");
                System.Console.WriteLine("       Elevator doors are opened");
                this.elevatorDoor.status = DoorStatus.OPENED;
                Door currentDoor = this.floorDoorsList.FirstOrDefault(door => door.id == this.floor); //filter floor door by ID and set status to OPENED
                if (currentDoor != null)
                {
                    currentDoor.status = DoorStatus.OPENED;
                }

                Thread.Sleep(1000); //How many time the door remains opened in MILLISECONDS - I use 1 second so the scenarios test will run faster                
                this.closeDoors();
            }

            /* ******* LOGIC TO CLOSE DOORS ******* */
            public void closeDoors()
            {
                this.checkWeight();
                this.checkObstruction();
                if (this.weightSensorStatus == SensorStatus.OFF && this.obstructionSensorStatus == SensorStatus.OFF)
                { //Security logic
                    System.Console.WriteLine("       Closing doors...");
                    System.Console.WriteLine("       Elevator doors are closed");
                    Door currentDoor = this.floorDoorsList.FirstOrDefault(door => door.id == this.floor); //filter floor door by ID and set status to OPENED
                    if (currentDoor != null)
                    {
                        currentDoor.status = DoorStatus.CLOSED;
                    }
                    this.elevatorDoor.status = DoorStatus.CLOSED;
                }
            }

            /* ******* LOGIC FOR WEIGHT SENSOR ******* */
            public void checkWeight()
            {
                int maxWeight = 500; //Maximum weight an elevator can carry in KG
                Random random = new Random();
                int randomWeight = random.Next(maxWeight + 100); //This random simulates the weight from a weight sensor
                while (randomWeight > maxWeight)
                {  //Logic of loading
                    this.weightSensorStatus = SensorStatus.ON;  //Detect a full elevator
                    System.Console.WriteLine("       ! Elevator capacity reached, waiting until the weight is lower before continue...");
                    randomWeight -= 100; //I'm supposing the random number is 600, I'll subtract 101 so it will be less than 500 (the max weight I proposed) for the second time it runs
                }
                this.weightSensorStatus = SensorStatus.OFF;
                System.Console.WriteLine("       Elevator capacity is OK");
            }

            /* ******* LOGIC FOR OBSTRUCTION SENSOR ******* */
            public void checkObstruction()
            {
                int probabilityNotBlocked = 70;
                Random random = new Random();
                int number = random.Next(100); //This random simulates the probability of an obstruction (I supposed 30% of chance something is blocking the door)
                while (number > probabilityNotBlocked)
                {
                    this.obstructionSensorStatus = SensorStatus.ON;
                    System.Console.WriteLine("       ! Elevator door is blocked by something, waiting until door is free before continue...");
                    number -= 30; //I'm supposing the random number is 100, I'll subtract 30 so it will be less than 70 (30% probability), so the second time it runs theres no one blocking the door
                }
                this.obstructionSensorStatus = SensorStatus.OFF;
                System.Console.WriteLine("       Elevator door is FREE");
            }

            /* ******* LOGIC TO ADD A FLOOR TO THE FLOOR LIST ******* */
            public void addFloorToFloorList(int floor)
            {
                if (!floorRequestList.Contains(floor))
                {
                    this.floorRequestList.Add(floor);
                    this.floorRequestList = this.floorRequestList.OrderBy(i => i).ToList(); //Order list ascending
                }
            }

            /* ******* LOGIC TO DELETE ITEM FROM FLOORS LIST ******* */
            public void deleteFloorFromList(int stopFloor)
            {
                int index = this.floorRequestList.IndexOf(stopFloor);
                if (index > -1)
                {
                    this.floorRequestList.RemoveAt(index);
                }
            }


            //----------------- Entry method -----------------//
            /* ******* ENTRY METHOD ******* */
            /* ******* REQUEST FOR A FLOOR BY PRESSING THE FLOOR BUTTON INSIDE THE ELEVATOR ******* */
            public void requestFloor(int requestedFloor)
            {
                // System.Console.WriteLine(" >> Someone inside the elevator" + this.id + " wants to go to floor <" + requestedFloor + "> <<");
                if (this.floor != requestedFloor)
                {
                    this.addFloorToFloorList(requestedFloor);
                    this.moveElevator(requestedFloor);
                }
            }
        }


        //------------------------------------------- DOOR CLASS --------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        public class Door
        {
            public int id;
            public DoorStatus status;
            public int floor;

            public Door(int doorId, DoorStatus doorStatus, int doorFloor)
            {
                id = doorId;
                status = doorStatus;
                floor = doorFloor;
            }
        }


        //------------------------------------------- BUTTON CLASS ------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        public class Button
        {
            public int id;
            public ButtonStatus status;
            public int floor;

            public Button(int buttonId, ButtonStatus buttonStatus, int buttonFloor)
            {
                id = buttonId;
                status = buttonStatus;
                floor = buttonFloor;
            }
        }


        //------------------------------------------- DISPLAY CLASS -----------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        public class Display
        {
            public int id;
            public DisplayStatus status;
            public int floor;

            public Display(int displayId, DisplayStatus displayStatus, int displayFloor)
            {
                id = displayId;
                status = displayStatus;
                floor = displayFloor;
            }
        }


        //------------------------------------------- ENUMS -------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        /* ******* BATTERY STATUS ******* */
        public enum BatteryStatus
        {
            ACTIVE,
            INACTIVE
        }

        /* ******* COLUMN STATUS ******* */
        public enum Status
        {
            ACTIVE,
            INACTIVE
        }

        /* ******* ELEVATOR STATUS ******* */
        public enum ElevatorStatus
        {
            IDLE,
            UP,
            DOWN
        }

        /* ******* BUTTONS STATUS ******* */
        public enum ButtonStatus
        {
            ON,
            OFF
        }

        /* ******* SENSORS STATUS ******* */
        public enum SensorStatus
        {
            ON,
            OFF
        }

        /* ******* DOORS STATUS ******* */
        public enum DoorStatus
        {
            OPENED,
            CLOSED
        }

        /* ******* DISPLAY STATUS ******* */
        public enum DisplayStatus
        {
            ON,
            OFF
        }

        /* ******* REQUESTED DIRECTION ******* */
        public enum Direction
        {
            UP,
            DOWN
        }


        //------------------------------------------- TESTING PROGRAM - SCENARIOS ---------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------------------------
        class Program
        {
            /* ******* CREATE SCENARIO 1 ******* */
            static void scenario1()
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                System.Console.WriteLine("\n****************************** SCENARIO 1: ******************************");
                Console.ResetColor();
                System.Console.WriteLine();
                Battery batteryScenario1 = new Battery(1, 4, 66, 6, 5, BatteryStatus.ACTIVE);
                System.Console.WriteLine(batteryScenario1);
                System.Console.WriteLine();
                batteryScenario1.columnsList.ForEach(column => System.Console.WriteLine(column));
                System.Console.WriteLine();
                //--------- ElevatorB1 ---------
                batteryScenario1.columnsList[1].elevatorsList[0].floor = 20;
                batteryScenario1.columnsList[1].elevatorsList[0].status = ElevatorStatus.DOWN;
                batteryScenario1.columnsList[1].elevatorsList[0].addFloorToFloorList(5);

                //--------- ElevatorB2 ---------
                batteryScenario1.columnsList[1].elevatorsList[1].floor = 3;
                batteryScenario1.columnsList[1].elevatorsList[1].status = ElevatorStatus.UP;
                batteryScenario1.columnsList[1].elevatorsList[1].addFloorToFloorList(15);

                //--------- ElevatorB3 ---------
                batteryScenario1.columnsList[1].elevatorsList[2].floor = 13;
                batteryScenario1.columnsList[1].elevatorsList[2].status = ElevatorStatus.DOWN;
                batteryScenario1.columnsList[1].elevatorsList[2].addFloorToFloorList(1);

                //--------- ElevatorB4 ---------
                batteryScenario1.columnsList[1].elevatorsList[3].floor = 15;
                batteryScenario1.columnsList[1].elevatorsList[3].status = ElevatorStatus.DOWN;
                batteryScenario1.columnsList[1].elevatorsList[3].addFloorToFloorList(2);

                //--------- ElevatorB5 ---------
                batteryScenario1.columnsList[1].elevatorsList[4].floor = 6;
                batteryScenario1.columnsList[1].elevatorsList[4].status = ElevatorStatus.DOWN;
                batteryScenario1.columnsList[1].elevatorsList[4].addFloorToFloorList(1);

                batteryScenario1.columnsList[1].elevatorsList.ForEach(elevator => System.Console.WriteLine(elevator));
                System.Console.WriteLine();
                System.Console.WriteLine("Person 1: (elevator B5 is expected)"); //elevator expected
                System.Console.WriteLine(">> User request an elevator from floor <1> and direction <UP> <<");
                System.Console.WriteLine(">> User request to go to floor <20>");
                batteryScenario1.columnsList[1].requestElevator(1, Direction.UP); //parameters (requestedFloor, buttonDirection.UP/DOWN)
                batteryScenario1.columnsList[1].elevatorsList[4].requestFloor(20); //parameters (requestedFloor)
                System.Console.WriteLine("=========================================================================");
            }

            /* ******* CREATE SCENARIO 2 ******* */
            public static void scenario2()
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                System.Console.WriteLine("\n****************************** SCENARIO 2: ******************************");
                Console.ResetColor();
                System.Console.WriteLine();
                Battery batteryScenario2 = new Battery(1, 4, 66, 6, 5, BatteryStatus.ACTIVE);
                System.Console.WriteLine(batteryScenario2);
                System.Console.WriteLine();
                batteryScenario2.columnsList.ForEach(column => System.Console.WriteLine(column));
                System.Console.WriteLine();
                //--------- ElevatorC1 ---------;
                batteryScenario2.columnsList[2].elevatorsList[0].floor = 1;
                batteryScenario2.columnsList[2].elevatorsList[0].status = ElevatorStatus.UP;
                batteryScenario2.columnsList[2].elevatorsList[0].addFloorToFloorList(21); //not departed yet

                //--------- ElevatorC2 ---------
                batteryScenario2.columnsList[2].elevatorsList[1].floor = 23;
                batteryScenario2.columnsList[2].elevatorsList[1].status = ElevatorStatus.UP;
                batteryScenario2.columnsList[2].elevatorsList[1].addFloorToFloorList(28);

                //--------- ElevatorC3 ---------
                batteryScenario2.columnsList[2].elevatorsList[2].floor = 33;
                batteryScenario2.columnsList[2].elevatorsList[2].status = ElevatorStatus.DOWN;
                batteryScenario2.columnsList[2].elevatorsList[2].addFloorToFloorList(1);

                //--------- ElevatorC4 ---------
                batteryScenario2.columnsList[2].elevatorsList[3].floor = 40;
                batteryScenario2.columnsList[2].elevatorsList[3].status = ElevatorStatus.DOWN;
                batteryScenario2.columnsList[2].elevatorsList[3].addFloorToFloorList(24);

                //--------- ElevatorC5 ---------
                batteryScenario2.columnsList[2].elevatorsList[4].floor = 39;
                batteryScenario2.columnsList[2].elevatorsList[4].status = ElevatorStatus.DOWN;
                batteryScenario2.columnsList[2].elevatorsList[4].addFloorToFloorList(1);

                batteryScenario2.columnsList[2].elevatorsList.ForEach(elevator => System.Console.WriteLine(elevator));
                System.Console.WriteLine();
                System.Console.WriteLine("Person 1: (elevator C1 is expected)"); //elevator expected
                System.Console.WriteLine(">> User request an elevator from floor <1> and direction <UP> <<");
                System.Console.WriteLine(">> User request to go to floor <36>");
                batteryScenario2.columnsList[2].requestElevator(1, Direction.UP); //parameters (requestedFloor, buttonDirection.UP/DOWN)
                batteryScenario2.columnsList[2].elevatorsList[0].requestFloor(36); //parameters (requestedFloor)
                System.Console.WriteLine("=========================================================================");
            }

            /* ******* CREATE SCENARIO 3 ******* */
            public static void scenario3()
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                System.Console.WriteLine("\n****************************** SCENARIO 3: ******************************");
                Console.ResetColor();
                System.Console.WriteLine();
                Battery batteryScenario3 = new Battery(1, 4, 66, 6, 5, BatteryStatus.ACTIVE);
                System.Console.WriteLine(batteryScenario3);
                System.Console.WriteLine();
                batteryScenario3.columnsList.ForEach(column => System.Console.WriteLine(column));
                System.Console.WriteLine();
                //--------- ElevatorD1 ---------
                batteryScenario3.columnsList[3].elevatorsList[0].floor = 58;
                batteryScenario3.columnsList[3].elevatorsList[0].status = ElevatorStatus.DOWN;
                batteryScenario3.columnsList[3].elevatorsList[0].addFloorToFloorList(1);

                //--------- ElevatorD2 ---------
                batteryScenario3.columnsList[3].elevatorsList[1].floor = 50;
                batteryScenario3.columnsList[3].elevatorsList[1].status = ElevatorStatus.UP;
                batteryScenario3.columnsList[3].elevatorsList[1].addFloorToFloorList(60);

                //--------- ElevatorD3 ---------
                batteryScenario3.columnsList[3].elevatorsList[2].floor = 46;
                batteryScenario3.columnsList[3].elevatorsList[2].status = ElevatorStatus.UP;
                batteryScenario3.columnsList[3].elevatorsList[2].addFloorToFloorList(58);

                //--------- ElevatorD4 ---------
                batteryScenario3.columnsList[3].elevatorsList[3].floor = 1;
                batteryScenario3.columnsList[3].elevatorsList[3].status = ElevatorStatus.UP;
                batteryScenario3.columnsList[3].elevatorsList[3].addFloorToFloorList(54);

                //--------- ElevatorD5 ---------
                batteryScenario3.columnsList[3].elevatorsList[4].floor = 60;
                batteryScenario3.columnsList[3].elevatorsList[4].status = ElevatorStatus.DOWN;
                batteryScenario3.columnsList[3].elevatorsList[4].addFloorToFloorList(1);

                batteryScenario3.columnsList[3].elevatorsList.ForEach(elevator => System.Console.WriteLine(elevator));
                System.Console.WriteLine();
                System.Console.WriteLine("Person 1: (elevator D1 is expected)"); //elevator expected
                System.Console.WriteLine(">> User request an elevator from floor <54> and direction <DOWN> <<");
                System.Console.WriteLine(">> User request to go to floor <1>");
                batteryScenario3.columnsList[3].requestElevator(54, Direction.DOWN); //parameters (requestedFloor, buttonDirection.UP/DOWN)
                batteryScenario3.columnsList[3].elevatorsList[0].requestFloor(1); //parameters (requestedFloor)
                System.Console.WriteLine("=========================================================================");
            }

            /* ******* CREATE SCENARIO 4 ******* */
            public static void scenario4()
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                System.Console.WriteLine("\n****************************** SCENARIO 4: ******************************");
                Console.ResetColor();
                System.Console.WriteLine();
                Battery batteryScenario4 = new Battery(1, 4, 66, 6, 5, BatteryStatus.ACTIVE);
                System.Console.WriteLine(batteryScenario4);
                System.Console.WriteLine();
                batteryScenario4.columnsList.ForEach(column => System.Console.WriteLine(column));
                System.Console.WriteLine();
                //--------- ElevatorA1 ---------
                batteryScenario4.columnsList[0].elevatorsList[0].floor = -4; //use of negative numbers to indicate SS / basement
                batteryScenario4.columnsList[0].elevatorsList[0].status = ElevatorStatus.IDLE;

                //--------- ElevatorA2 ---------
                batteryScenario4.columnsList[0].elevatorsList[1].floor = 1;
                batteryScenario4.columnsList[0].elevatorsList[1].status = ElevatorStatus.IDLE;

                //--------- ElevatorA3 ---------
                batteryScenario4.columnsList[0].elevatorsList[2].floor = -3; //use of negative numbers to indicate SS / basement
                batteryScenario4.columnsList[0].elevatorsList[2].status = ElevatorStatus.DOWN;
                batteryScenario4.columnsList[0].elevatorsList[2].addFloorToFloorList(-5);

                //--------- ElevatorA4 ---------
                batteryScenario4.columnsList[0].elevatorsList[3].floor = -6; //use of negative numbers to indicate SS / basement
                batteryScenario4.columnsList[0].elevatorsList[3].status = ElevatorStatus.UP;
                batteryScenario4.columnsList[0].elevatorsList[3].addFloorToFloorList(1);

                //--------- ElevatorA5 ---------
                batteryScenario4.columnsList[0].elevatorsList[4].floor = -1; //use of negative numbers to indicate SS / basement
                batteryScenario4.columnsList[0].elevatorsList[4].status = ElevatorStatus.DOWN;
                batteryScenario4.columnsList[0].elevatorsList[4].addFloorToFloorList(-6); //use of negative numbers to indicate SS / basement

                batteryScenario4.columnsList[0].elevatorsList.ForEach(elevator => System.Console.WriteLine(elevator));
                System.Console.WriteLine();
                System.Console.WriteLine("Person 1: (elevator A4 is expected)"); //elevator expected
                System.Console.WriteLine(">> User request an elevator from floor <-3> (basement) and direction <UP> <<");
                System.Console.WriteLine(">> User request to go to floor <1>");
                batteryScenario4.columnsList[0].requestElevator(-3, Direction.UP); //parameters (requestedFloor, buttonDirection.UP/DOWN)
                batteryScenario4.columnsList[0].elevatorsList[3].requestFloor(1); //parameters (requestedFloor)
                System.Console.WriteLine("=========================================================================");
            }

            //------------------------------------------- TESTING PROGRAM - CALL SCENARIOS -----------------------------------------------------
            //----------------------------------------------------------------------------------------------------------------------------------
            static void Main(string[] args)
            {

                /* ******* CALL SCENARIOS ******* */
                // scenario1();
                // scenario2();
                // scenario3();
                // scenario4();
            }
        }
    }
}