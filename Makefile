sources := $(wildcard src/**/*.cs)
scripts := $(wildcard scripts/prepres/*) scripts/build-prepped scripts/build-prepped_dbtemplate

bin/prep/packages/icecraft/unitary/IceCraft: $(sources) $(scripts)
	scripts/build-prepped

clean:
	rm -rf bin/prep/

install: bin/prep/packages/icecraft/unitary/IceCraft
	cd bin/prep;./install.sh -y
