# MartinezClipper
A C# implementation of a polygon clipping algorithm that enables several boolean operations (union, difference, exclusive-or and intersection) on polygons.

The code is based on an algorithm described in a paper from 2009 called: *A new algorithm for computing Boolean operations on polygons by Francisco Martı́nez, Antonio Jesús Rueda and Francisco Ramón Feito*.

The time complexity for this algorithm is __O((n + k) log n)__ where *k* is the number of intersections of all the polygon edges *n*.

Please note: Martínez published a new paper titled *A simple algorithm for Boolean operations on polygons* with a new version of the algorithm in 2013.
