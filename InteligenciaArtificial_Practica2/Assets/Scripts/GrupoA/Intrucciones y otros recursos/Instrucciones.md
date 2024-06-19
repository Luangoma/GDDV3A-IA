## Nomenclatura de la tabla Q
La nomenclatura del fichero de la tablaQ es la siguiente:
"QTable_Var1_Var2_Var3_Var4_Ver5"
- Var1: Es el numero de episodios con los que el agente se ha entrenado. Debido a que es un numero alto, se representa como #K, siendo # el numero de episodios entre mil.
- Var2: Es el numero de distancias discretas con las que el agente se entrena. Se representa como #D siendo # el numero correspondiente.
- Var3: Es el numero de angulos discretos con los que el agente se entrena. Se representa como #A siendo # el numero correspondiente.
- Var4: Un binario donde si es 1 indica que se ha entrenado poniendo inicialmente recompensas negativas y 0 si inicialmente todos los valores son vacíos, es decir, 0.
- Ver5: Version de la tabla, se incrementa en caso de un entrenamiento erroneo y se deba volver a un punto anterior

## Escenarios
Hay 8 escenarios:
### Escenario 1 - Nivel 1
Este escenario es el predeterminado del proyecto. Sirve para un comienzo
### Escenario 2 - Nivel 2
Este escenario es un remodelado del primero, donde los muros se han cambiado de posicion.
### Escenario 3 - Nivel 3
Este escenario es un nivel mas complejo divido en cuatro estancias, donde se se presenten situaciones mas variadas al agente.
### Escenario 4 - Llanura
Este escenario sirve para entrenar el agente en un espacio abierto para que aprenda a que hacer respecto a donde esta el agente.
### Escenario 5 - Bordes
Este escenario esta limitado a solamente los bordes para que el agente aprenda a que hacer cuando este acorralado o con movimientos limitados.
### Escenario 6 - Centro
Este escenario es parecido al escenario 5, pero con la variacion de que los espacios con los bordes respecto al mapa, son mayores para darle al agente margen de fallo.
### Escenario 7 - Redux
Este escenario reducido se centra en el entrenamiento del agente bajo presion donde el perseguidor lo tiene siempre cerca.
### Escenario 8 - Donut
Este escenario es una mezcla de Bordes y Redux, de forma que se entrene ambas caracteristicas.
### Escenario 9 - Laberinto
Este escenario intenta simular un laberinto de forma 
### Escenario 10 - Diagonales A1
Este escenario fuerza al agente a aprender de estados donde este en esquinas en cierta orientacion
### Escenario 11 - Diagonales A2
Este escenario fuerza al agente a aprender de estados donde este en esquinas en cierta orientacion. 
Es una ampliación de Diagonales A1.
### Escenario 12 - Diagonales B1
Este escenario fuerza al agente a aprender de estados donde este en esquinas en cierta orientacion
### Escenario 13 - Diagonales B2
Este escenario fuerza al agente a aprender de estados donde este en esquinas en cierta orientacion 
Es una ampliación de Diagonales B1.

Para otras ideas se puede usar blender con el archivo base, que es una plantilla del terreno de juego para crear nuevos niveles.

## Episodios por escenario
Se adjunta una tabla con los episodios recomendados por cada escenario. 
| Id del escenario | Episodios recomendados |
| -: | -: |
| 1 |  20 000 |
| 2 |  20 000 |
| 3 |  40 000 |
| 4 |  40 000 |
| 5 |  40 000 |
| 6 |  40 000 |
| 7 |  60 000 |
| 8 |  80 000 |
| 9 | 100 000 |

## Guardados
Se recomienda guardar antes de cada prueba de forma que haya una copia anterior en caso de que haya algun entrenamiento que perjudique el progreso del agente.

## Empleo de la tabla Q guardada en el programa
Para usar una de las tablas guardadas el archivo debe llamarse 'QTable.cs'. Tal y como esta escrito el programa, solamente lee archivos de esta forma.