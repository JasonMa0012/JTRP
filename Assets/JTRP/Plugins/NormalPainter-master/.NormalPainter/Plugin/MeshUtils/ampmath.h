#pragma once

#include <amp.h>
#include <amp_graphics.h>
#include <amp_math.h>


// fast math
namespace apm {
using namespace concurrency;
using namespace concurrency::graphics;
using namespace concurrency::fast_math;
#include "ampmath_impl.h"
} // namespace apm


// precise math
namespace afm {
using namespace concurrency;
using namespace concurrency::graphics;
using namespace concurrency::precise_math;
#include "ampmath_impl.h"
} // namespace afm
