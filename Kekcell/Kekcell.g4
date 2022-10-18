grammar Kekcell;

compileUnit : expression EOF;

expression :
	LPAREN expression RPAREN #ParenthesizedExpr
	| SUBTRACT expression #UnaryMinus
	| token=(PI | E) #ConstExpr
	| expression EXPONENT expression #ExponentialExpr
	| expression operatorToken=(MULTIPLY | DIVIDE | MOD) expression #MultiplicativeExpr
	| expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr
	| operatorToken=(
		MAX | MIN 
		| AVERAGE | MEDIAN | MODE
		| SUM | PRODUCT
	  ) LPAREN paramlist=arguments RPAREN #StatisticFuncExpr
	| operatorToken=(
		ROUND | CEIL | FLOOR 
		| SIN | COS | TAN | COT | SINH | COSH | TANH | COTH
		| LOG2 | LOG10 | LOG | EXP
		| SQRT | ABS
	  ) LPAREN expression RPAREN #NumberFuncExpr
	| LOG LPAREN expression ',' expression RPAREN #LogExpr
	| LPAREN expression RPAREN 'deg' #DegreeExpr
	| NUMBER #NumberExpr
	| CELL #CellExpr
	| DEGREE #DegreeNumberExpr
	;

arguments : expression (',' expression)*; 
paramlist : expression (',' expression)*; 

NUMBER : INT ('.' INT)?; 
CELL : [A-Z]+[1-9]+[0-9]*; 
INT : ('0'..'9')+; 
DEGREE : NUMBER 'deg';

PI: 'pi';
E: 'e';

EXPONENT : '^';
MULTIPLY : '*';
DIVIDE : '/';
MOD : '%';
SUBTRACT : '-';
ADD : '+';
LPAREN : '(';
RPAREN : ')';
MAX : 'Max';
MIN : 'Min';
AVERAGE : 'Average';
MEDIAN : 'Median';
MODE : 'Mode';
SUM : 'Sum';
PRODUCT : 'Product';
ROUND : 'Round';
CEIL : 'Ceil';
FLOOR : 'Floor';
SIN : 'Sin';
COS : 'Cos';
TAN : 'Tan';
COT : 'Cot';
SINH : 'Sinh';
COSH : 'Cosh'; 
TANH : 'Tanh';
COTH : 'Coth';
LOG2 : 'Log2'; 
LOG10 : 'Log10';
LOG : 'Log';
EXP : 'Exp';
SQRT : 'Sqrt';
ABS : 'Abs';

WS : [ \t\r\n] -> channel(HIDDEN);