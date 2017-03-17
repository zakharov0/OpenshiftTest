FROM rhel7
# This image provides a .NET Core 1.1 environment you can use to run your .NET
# applications.

EXPOSE 8080

ENV DOTNET_CORE_VERSION=1.1
# Default to UTF-8 file.encoding
ENV LANG=C.UTF-8 \
    HOME=/opt/app-root \
    PATH=/opt/app-root/src/.local/bin:/opt/app-root/src/bin:/opt/app-root/bin:/opt/app-root/node_modules/.bin:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin \
    STI_SCRIPTS_PATH=/usr/libexec/s2i \
    DOTNET_PUBLISH_PATH=/opt/app-root/publish \
    DOTNET_RUN_SCRIPT=/opt/app-root/publish/s2i_run

LABEL io.k8s.description="Platform for building and running .NET Core 1.1 applications" \
      io.k8s.display-name=".NET Core 1.1" \
      io.openshift.s2i.scripts-url=image:///usr/libexec/s2i \
      io.openshift.tags="builder,.net,dotnet,dotnetcore,rh-dotnetcore11" \
      io.openshift.expose-services="8080:http" \
      io.s2i.scripts-url=image:///usr/libexec/s2i

# Labels consumed by Red Hat build service
LABEL name="dotnet/dotnetcore-11-rhel7" \
      com.redhat.component="rh-dotnetcore11-docker" \
      version="1.1" \
      release="2" \
      architecture="x86_64"
      
COPY ./root/usr/bin /usr/bin

# Copy the S2I scripts from the specific language image to $STI_SCRIPTS_PATH.
COPY ./s2i/bin/ /usr/libexec/s2i

# Each language image can have 'contrib' a directory with extra files needed to
# run and build the applications.
COPY ./contrib/ /opt/app-root

USER 0

# Directory with the sources is set as the working directory. This should
# be a folder outside $HOME as this might cause issues when compiling sources.
# See https://github.com/redhat-developer/s2i-dotnetcore/issues/28
WORKDIR /opt/app-root/src

# Run container by default as user with id 1001 (default)
#USER 1001

# By default, ASP.NET Core runs on port 5000. We configure it to match
# the container port.
ENV ASPNETCORE_URLS=http://*:8080

ENTRYPOINT ["container-entrypoint"]

# Set the default CMD to print the usage of the language image.
CMD /usr/libexec/s2i/usage
