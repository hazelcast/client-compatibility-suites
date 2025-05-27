FROM fedora:22

RUN dnf groups install -y "Development Tools"
RUN dnf install -y gcc-c++ tar wget bzip2 unzip cmake which

RUN wget --quiet https://github.com/Kitware/CMake/releases/download/v3.19.0/cmake-3.19.0-Linux-x86_64.sh && \
    chmod +x ./cmake-3.19.0-Linux-x86_64.sh && \
    ./cmake-3.19.0-Linux-x86_64.sh --prefix=/usr/local/ --exclude-subdir && \
    rm cmake-3.19.0-Linux-x86_64.sh
	
RUN wget --quiet https://archives.boost.io/release/1.76.0/source/boost_1_76_0.tar.bz2 && tar xjf boost_1_76_0.tar.bz2 && rm boost_1_76_0.tar.bz2 && cd boost_1_76_0 && ./bootstrap.sh && ./b2 address-model=64 --with-thread --with-chrono install && cd .. && rm -rf boost_1_76_0

ADD . .

RUN cd clientSourceCode && mkdir build && cd build && cmake .. && cmake --build . && cmake --build . --target install

RUN mkdir build && cd build && cmake .. && cmake --build .

CMD ["./build/cppClientProject"]

