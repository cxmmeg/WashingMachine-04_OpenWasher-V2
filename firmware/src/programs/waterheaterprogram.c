/*
 * ������ ���� � ����
 */

#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>
#include "delay.h"
#include "door_stages.h"
#include "pump_driver.h"
#include "therm_driver.h"
#include "valve_driver.h"
#include "options.h"

extern volatile bool ct;

bool waterheater_go(options args)
{
	//t
	uint8_t temperature = args.temperature != 0xFF ? args.temperature : 70;
	if(temperature > 80)
	{
		printf("Tmax = 80\n");
		return false;
	}

	//waterlevel
	uint8_t waterlevel = args.waterlevel != 0xFF ? args.waterlevel : 50;
	if(waterlevel > 100)
		waterlevel = 100;

	printf("Start water heater at t: %u, water level: %u\n", temperature, waterlevel);

	if(!stage_door_close())
		return false;

	while(!ct)
	{
		if(!valve_drawwater(conditioner_valve, waterlevel))
			break;

		if(!set_temperature(70))
			break;

		while(get_temperature() < 70)
		{
			printf("T %u\n", get_temperature());
			delay_ms(10000u);
		}

		if(!set_temperature(0))
			break;

		if(!sink(15000))
			break;

		delay_ms(30000u);
	}

	if(!set_temperature(0))
		return false;

	if(!sink(15000))
		return false;

	if(!stage_door_open())
		return false;

	return true;
}


