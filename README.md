## Название работы и ФИО автора

Лабораторная работа 7. Анализ и преобразование кода с использованием Clang и LLVM  
Коченков А. А.

## Постановка задачи
   
1. Установка среды  
  Установить Clang, LLVM, opt и Graphviz (например, в Ubuntu 26.04).
2. Работа с AST  
  Сгенерировать абстрактное синтаксическое дерево для заданного C/C++‑файла.
3. Генерация LLVM IR  
  Получить промежуточное представление кода без оптимизаций (-O0) и с оптимизациями (-O2).
4. Оптимизация IR  
  Применить оптимизации с помощью opt и/или флагов Clang, сравнить изменения.
5. Построение CFG  
  Построить граф потока управления для одной или нескольких функций.
6. Индивидуальное задание (по варианту)  
  Выполнить анализ конкретной синтаксической конструкции в соответствии с вариантом.
  Сформулировать, как LLVM обрабатывает выбранную конструкцию, какие оптимизации применяются.
7. Выводы  
  Ответить на контрольные вопросы

## 1. Общее задание

### 1.1 Исходный код

```c
#include <stdio.h>

int square(int x) {
  return x * x;
}

int main() {
  int a = 5;
  int b = square(a);
  printf("%d\n", b);
  return 0;
}
```

В коде определяется функция square, вычисляющая квадрат полученного числа. В main вызывается square для значения 5 и выводится результат.

### 1.2 Получение AST

Команда для генерации AST:
`clang -Xclang -ast-dump -fsyntax-only main.c`

<img width="1026" height="766" alt="Изображение AST" src="https://github.com/user-attachments/assets/164100e6-6137-4f20-949a-319a512b0213" />

В AST функция square представлена узлом FunctionDecl, параметр x — узлом ParmVarDecl, а выражение x * x — узлом BinaryOperator.

### 1.3 Генерация LLVM IR

LLVM IR до оптимизаций:  
`clang -O0 -S -emit-llvm main.c -o main_O0.ll`  
<img width="1022" height="768" alt="LLVM IR до оптимизаций" src="https://github.com/user-attachments/assets/7f67260e-2d6a-4fe9-a3b0-78efebcc3308" />

LLVM IR после оптимизации -O2:  
`clang -O2 -S -emit-llvm main.c -o main_O2.ll`  
<img width="1024" height="766" alt="LLVM IR после оптимизаций" src="https://github.com/user-attachments/assets/fc90be1d-72c5-426b-b275-7866d5de3c69" />

В IR до оптимизаций присутствуют инструкции alloca, store, load, а функция square вызывается отдельно.  
После оптимизации -O2 код становится короче: компилятор удаляет лишние инструкции, может встроить функцию square и упростить вычисления.

#### Сравнение IR

Команда сравнения:  
`diff -u main_O0.ll main_O2.ll > main_diff.txt`

<img width="1026" height="766" alt="diff main_O0 main_O2" src="https://github.com/user-attachments/assets/3f683868-3ac9-40b9-aef9-eda8c91292a6" />

После оптимизации уменьшается количество инструкций, исчезают лишние обращения к памяти, а вычисления упрощаются.

### 1.4 Построение CFG

Команда для генерации .dot-файлов CFG для функций до оптимизаций:  
`opt -passes=dot-cfg -disable-output main_O0.ll`

Команды для преобразования файлов с расширением .dot в .png с помощью Graphviz:  
```
dot -Tpng .main.dot -o cfg_main_O0.png
dot -Tpng .square.dot -o cfg_square_O0.png
```

<img width="782" height="304" alt="cfg_main_O0.png" src="https://github.com/user-attachments/assets/ee2fcacd-cea9-40f4-a30c-7de838d90ead" />  
<img width="375" height="205" alt="cfg_square_O0.png" src="https://github.com/user-attachments/assets/09f55fe4-e3db-4bc1-84fd-cfc6b86451a1" />

Построение CFG после -O2:  
```
opt -passes=dot-cfg -disable-output main_02.ll
dot -Tpng .main.dot -o cfg_main_02.png
dot -Tpng .square.dot -o cfg_square_02.png
```

<img width="661" height="144" alt="cfg_main_O2.png" src="https://github.com/user-attachments/assets/31fc3b4c-0d52-4210-8005-aed25cdce0da" />  
<img width="286" height="125" alt="cfg_square_O2.png" src="https://github.com/user-attachments/assets/cfeac2f6-4223-4eb4-93be-7d87256b1b79" />

CFG строится отдельно для каждой функции. После оптимизации граф становится проще, так как часть инструкций удаляется, а короткие функции могут быть встроены.

## 2. Индивидуальное задание

### 2.1 Исходный код

Вариант 2.7 Вещественные константы

```c
const double PI = 3.141592653589793;

int main() {
  double r = 2.0;
  double area = PI * r * r;
  return (int)area;
}
```

### 2.2 Получение IR -O0

Используемая команда:  
`clang -O0 -S -emit-llvm main.c -o main_O0.ll`

<img width="1026" height="606" alt="main_O0.ll" src="https://github.com/user-attachments/assets/1230273b-305c-4a67-aed7-c53104098022" />

### 2.3 Получение IR для -O2

Используемая команда:  
`clang -O2 -S -emit-llvm main.c -o main_O2.ll`

<img width="1028" height="392" alt="main_O2.ll" src="https://github.com/user-attachments/assets/19008c6c-934f-4ec5-ad24-b5dec50b8426" />

Произошло свертывание константы - всё вещественное выражение и последующее преобразование к int вычислены компилятором, поэтому в итоговом IR остаётся только `ret i32 12`.

### 2.4 Использование -constprop, -globalopt, -ipsccp

В современном llvm проход `constprop` отсутствует, вместо него для распространения констант используется `sccp`

Используемая команда:  
`opt -passes='function(sccp),globalopt,ipsccp,function(dot-cfg)' -disable-output main_O0.ll`

Полученный CFG:  
<img width="507" height="345" alt="cfg-main_O0_opt.png" src="https://github.com/user-attachments/assets/893778dc-8a18-4f8b-84c5-4342778eb7f3" />

### 2.5 Сравнение CFG


### 2.6 Вывод



## Дополнительное задание

