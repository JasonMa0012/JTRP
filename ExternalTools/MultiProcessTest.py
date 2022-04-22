from multiprocessing import Pool

import os
import time


def long_time_task(name):
    print('Run task %s (%s)...' % (name, os.getpid()))
    start = time.time()
    time.sleep(1)
    end = time.time()
    print('Task %s runs %0.2f seconds.' % (name, (end - start)))


if __name__ == '__main__':
    print('Parent process %s.' % os.getpid())
    
    p = Pool()
    
    for i in range(100):
        p.apply_async(long_time_task, args=(i,))
        
    print('Waiting for all subprocesses done...')
    
    p.close()
    p.join()
    
    print('All subprocesses done.')
