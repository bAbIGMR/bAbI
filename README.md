# GMR Introduction
GMR (Graph Matching based Reasoner) to the bAbI challenge -- we automatically generate reasoning rules in the form of graphs from 
the training set and use the generated rules to infer answers to the questions in the test set via graph matching. Using this symbolic approach, all the 20 question answering tasks of bAbI are solved with average accuracy 99.38% using one single jointly trained model. Compared with the neural network based approaches, the proposed symbolic approach generalizes better -- it can achieve better accuracy with much fewer training samples, more versatile -- it can well handle non-trivial logical reasoning problems that neural network
based approaches generally struggle to handle, and more robust and stable -- it performs stably for all tasks with zero test variance.
# Run GMR
## Download Data and Essential Packages
bAbI tasks data 1-20 (v1.2) https://research.fb.com/downloads/babi/ <br> 
Graph Engine Package https://github.com/Microsoft/GraphEngine <br> 
Stanford CoreNLP package https://stanfordnlp.github.io/CoreNLP/ <br> 
IKVM package https://www.ikvm.net/ <br> 
Newtonsoft.Json package https://www.newtonsoft.com/json <br> 
## Run Steps
DataPreprocessing (process bAbI dataset and save it as Graph Engine storage): "GMR.exe -e" <br>
RuleGeneration (generate rule graph set): "GMR.exe -g" <br>
Test (get each task accuracy on test data): "GMR.exe -t" <br>
