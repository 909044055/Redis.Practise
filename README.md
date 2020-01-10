# RedisLock.Practise
  practise redis lock 

##Install CentOS7 Virtual Machine

##Show ip info

    $ ifconfig

  If ifconfig command not found

    $ yum search ifconfig

Found ifconfig command in package net-tools.x86_64
  
    $ yum install net-tools.x86_64


##Download, extract and compile Redis with:

    $ wget http://download.redis.io/releases/redis-5.0.7.tar.gz
    $ tar xzf redis-5.0.7.tar.gz
    $ cd redis-5.0.7
    $ make

  If make error,it maybe gcc is not install yet
    $ yum install gcc

  Make again,If show some file not found message,you can extract again.


##Edit redis default config:

    $ vi redis.conf 

##set password 

    requirepass <password>


##bind ip or disable protected mode to allow remote access redis server

    bind 192.168.5.40
    
or

    protected-mode no



##Configure firewall

    $ iptables -F
    
  or
    
    $ iptables -A INPUT -p tcp --dport 6379 -s 192.168.5.59 -j ACCEPT
    
  or
    
    $ iptables -A INPUT -p tcp --dport 6379 -j ACCEPT
