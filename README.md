# vb_compiler
Compiler Visual Basic programming language

A Visual Basic translator whose input data is a program written on a subset of the VB language.
```
This subset of VB:
```
* Case insensitive identifiers, the value is the first 8 characters.
* Directive for the description of variables Byte, Integer, long.
* The statement of the do-do loop, the exit do statement.
* A simple arithmetic operator.
```
Developed translator must perform the following tasks:
```
* accept the source code of the program (in a text file or typed directly into the program);
* lexical analysis;
* parsing;
* Provide output of intermediate information after each stage of the broadcast;
* Ensure the output of messages in the source text (type of error and place in the source text);
```
Other requirements for the project:
```
* Provide detailed diagnostics of errors;
* syntactic analysis based on LR-grammars;
* analysis of complex logical expression by the method of Dijkstra;
