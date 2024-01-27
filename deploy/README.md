# Развёртывание локального окружения
> Все приведённые ниже команды должны выполняться в терминале.
1. Клонировать репозиторий `git clone https://github.com/echo-4droid/nope-cha.git`.
2. Установить Docker Desktop по [официальному мануалу](https://docs.docker.com/desktop/install/ubuntu/) (или по [мануалу timeweb](https://timeweb.cloud/tutorials/docker/kak-ustanovit-docker-na-ubuntu-22-04)).
3. Перейти в каталог с клонированным репозиторием.
3. Построить образ `docker build . -f deploy/Dockerfile -t nopecha`.
4. Запустить контейнер `docker run --ipc=host --name nopecha -d -p 5000:80 -v nopecha:/app nopecha`.