import re

f = open('skillstr.txt')
f1 = open('skills.dat', 'w')

for line in f:
    match = re.match('^(?P<id>\d{2,5})  "(?P<name>.*?)"$', line)
    if match and int(match.group('id')) % 10 == 0:
        id = int(match.group('id')) / 10
        name = match.group('name')
        f1.write(str(id) + ' - ' + name + '\n')

f1.close()
