from gest_base import *
import os
import pygame
import math
import random
import matplotlib.pyplot as plt
from dataclasses import dataclass
from contextlib import contextmanager
import timeit
import copy
import numpy as np
import pickle
from typing import Callable
from abc import abstractmethod
from utils import *
from linear_next_approx_algo import LinearNextApproxAlgo
from point_pre_processors import RemoveDupesPointPreProcessor


if __name__ == "__main__":
    #make_gesture()

    algo = LinearNextApproxAlgo()
    run_multi_matching_demo(["g_fire", "g_ice", "g_lightning", "g_wind"], algo, algo, RemoveDupesPointPreProcessor())

    #g1 = [GestComp(math.pi / 10, 3), GestComp(math.pi / 8, 3), GestComp(math.pi / 6, 2), GestComp(math.pi / 2, 2), GestComp(math.pi / 2, 2)]
