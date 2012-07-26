# -*- coding: utf8 -*-

from argparse import ArgumentParser
from grab import Grab

parser = ArgumentParser()
parser.add_argument('dbver',
                    nargs='?',
                    help='database version (pwi, ru, ...)')
args = parser.parse_args()

print args.dbver
    
f = open('items_{}.dat'.format(args.dbver), 'w')

g = Grab()
for id in range(0, 35000):
    g.go("http://www.pwdatabase.com/{}/{}/{}".format(args.dbver, 'items', id))
    if g.xpath('//title').text != 'Perfect World Item Database':
        name = g.xpath_text('//th[@class="itemHeader"]').rstrip(u' ▼')
        string = u'{}={}'.format(id, name)
        print id
        f.write((string + '\n').encode('utf8'))

f.close()
