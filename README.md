# Flocking
Flocking simulations and mini game

## Author
Minas Mina  
Contact: [minasm1990 [at] gmail.com](mailto:minasm1990@gmail.com)  

## About this project
_Flocking_ is a project that implements steering behaviors of flocks of birds. It has three modes:
* Wander Behavior
* Shape Formation
* "Gather the Flock" mini game

The implementation of the steering behaviors can be found in: 
```
\source\Assets\SteeringBehaviors
```
The behaviors can be used autonomously, even in non-GUI applications.

## Modes
### Wander Behavior
In this mode the user can see how the parameters for _cohesion_, _separation_ and _velocity match_ affect the formation of a flock that follows a leader (controlled by the user).

By pressing the _ESC_ key, the simulation pauses and you can see how you can control the leader bird, rotate the camera etc.

![Alt text](/screenshots/wander behavior.png?raw=true "Wander Behavior mode")