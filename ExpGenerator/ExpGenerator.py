#ExpGenerator.py
from jinja2 import Environment, FileSystemLoader
import os
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

class Expr:

    def __init__(self, name):
        self.name = name
        self.fields = {}

    def add_field(self, fieldName, fieldType):
        self.fields[fieldName] = fieldType

env = Environment(
    loader=FileSystemLoader(f'{BASE_DIR}/ExpGenerator/templates')
)

exprs = []

with open(f'{BASE_DIR}/ExpGenerator/expressions.txt') as f:
    for line in f:
        exprStr = line.split(':')
        exprFields = exprStr[1].split(',')
        exprInst = Expr(exprStr[0].strip())

        for field in exprFields:
            fieldTokes = field.split()
            exprInst.add_field(fieldTokes[1].strip(), fieldTokes[0].strip())

        exprs.append(exprInst)

template = env.get_template("exp.jinja")

outputDir = f'{BASE_DIR}/SeeSharp/Expressions/Autogenerated'
if not os.path.isdir(outputDir):
    os.mkdir(outputDir)

for expr in exprs:
    with open(f"{outputDir}/{expr.name}.cs", "w") as fh:
        fh.write(template.render(
            exprName = expr.name,
            fields = expr.fields
            )
        )

template = env.get_template("visitor.jinja")
exprNames = []
for expr in exprs:
    exprNames.append(expr.name)

with open(f"{BASE_DIR}/SeeSharp/Expressions/Autogenerated/IExprVisitor.cs", "w") as fh:
    fh.write(template.render(
        exprNames = exprNames
        )
    )