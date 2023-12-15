# GDDV3A-InteligenciaArtificial
## Práctica 1
El objetivo de la práctica es implementar un agente inteligente y los algoritmos de búsqueda necesarios para que este se desenvuelva dentro de un mundo virtual. La práctica se implementará utilizando el motor de videojuegos Unity™ y su lenguaje de scripting C#. Esta práctica requerirá desarrollar las clases que permitan resolver los distintos problemas que vamos a plantear dentro del escenario.

### Objetivos específicos de la practica
#### Búsqueda del camino de salida (6 puntos)
El agente seleccionará un único objetivo, la salida (pueden desactivarse todos los demás objetivos y enemigos), y desarrollará el algoritmo de búsqueda de caminos encontrará el camino hasta la meta. En dicho algoritmo se emplearán las heurísticas que se consideren oportunas.  
Para este apartado se ha Implementado el algoritmo A* y la distancia Manhattan.

#### Búsqueda por subobjetivos (2 puntos)
El agente seleccionará los cofres y la meta, y decidirá en que orden deben encontrarse de forma eficiente, para recoger todos los cofres y finalizar en la meta. Posteriormente empleará el algoritmo de busqueda ya definido para realizar esta tarea.

Se logró implementar la busqueda por subobjetivos.

#### Atrapar a los enemigos (2 puntos)
Ahora el agente deberá atrapar a los zombies, además de recoger los cofres y finalizar en la meta. Como los zombies se desplazan por la escena, empleará búsqueda online con el algoritmo de búsqueda de caminos ya implementado.

Este último no se logró implementar por falta de tiempo. Se hubiera implementado primero buscando a los jugadores rivales (zombis) usando un busqueda online (por horiozonte de profundidad 3) y una vez capturado todos se procede a los subobjetivos y por ultimo la meta (como anteriormente)


## Práctica 2
El objetivo de la práctica es implementar un agente inteligente cuyo comportamiento sea aprendido por técnicas de machine learning. En concreto, se emplearán tecnicas de Q-Learning para entrenar el cerebro del agente. La práctica se desarrollará utilizando el motor de videojuegos Unity™ y su lenguaje de scripting C#. Esta práctica requerirá desarrollar las clases que permitan aprender y ejecutar el comportamiento que resuelva el objetivo que vamos a plantear.

### Objetivos específicos de la práctica
#### Programar el sistema de entrenamiento del agente (10 puntos)
El agente (Agent) deberá evitar ser atrapado por el rival (Player). Para ello, deberá ser entrenado mediante técnicas de QLearning.
 
| Promedio de pasos | Calificación |
| ----------------- | ------------ |
| 250-1000          | 6            |
| 1000-5000         | 8            |
| >5000             | 10           |